using BiddingService.Data;
using BiddingService.Entities;
using BiddingService.Exceptions;
using BiddingService.Services;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BiddingService.Consumers;

public class BidPlacedConsumer : IConsumer<RequestBidPlace>
{
    private readonly BidDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public BidPlacedConsumer(IPublishEndpoint publishEndpoint, BidDbContext dbContext)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<RequestBidPlace> context)
    {
        var auction = await _dbContext.Auctions.FirstOrDefaultAsync(p => p.Id == context.Message.AuctionId);
        if (auction == null)
        {
            throw new PlaceBidException(
            context.Message.Bidder,
            context.Message.Amount,
            context.Message.AuctionId.ToString(),
            "Невозможно назначить заявку на этот аукцион - аукцион не найден!"
        );
        }

        if (auction.Seller == context.Message.Bidder) throw new PlaceBidException(
            context.Message.Bidder,
            context.Message.Amount,
            context.Message.AuctionId.ToString(),
            "Невозможно подать предложение для собственного аукциона"
        );

        var bid = new Bid
        {
            Amount = context.Message.Amount,
            AuctionId = context.Message.AuctionId,
            Bidder = context.Message.Bidder
        };

        if (auction.AuctionEnd < DateTime.UtcNow)
        {
            bid.BidStatus = BidStatus.Завершено;
        }
        else
        {
            var highBid = await _dbContext.Bids.Where(p => p.AuctionId == context.Message.AuctionId)
                .OrderByDescending(p => p.Amount).FirstOrDefaultAsync();

            if ((highBid != null && context.Message.Amount > highBid.Amount) || highBid == null)
            {
                bid.BidStatus = BidStatus.Принято;
            }
        }
        await _dbContext.Bids.AddAsync(bid);
        await _dbContext.SaveChangesAsync();

        await _publishEndpoint.Publish(new BidPlaced(
            context.Message.CorrelationId,
            context.Message.AuctionId,
            context.Message.Amount));

        Console.WriteLine("--> Получение сообщения - размещена заявка - " +
                 context.Message.Bidder + ", " + context.Message.Amount);
    }
}
