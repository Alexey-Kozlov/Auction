using BiddingService.Data;
using BiddingService.Entities;
using BiddingService.Exceptions;
using BiddingService.Services;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BiddingService.Consumers;

public class BidPlacingConsumer : IConsumer<BidPlacing>
{
    private readonly BidDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public BidPlacingConsumer(IPublishEndpoint publishEndpoint, BidDbContext dbContext)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<BidPlacing> context)
    {
        var auction = await _dbContext.Auctions.FirstOrDefaultAsync(p => p.Id == context.Message.Id);
        if (auction == null)
        {
            throw new PlaceBidException(
            context.Message.Bidder,
            context.Message.Amount,
            context.Message.Id.ToString(),
            "Невозможно назначить заявку на этот аукцион - аукцион не найден!"
        );
        }

        if (auction.Seller == context.Message.Bidder) throw new PlaceBidException(
            context.Message.Bidder,
            context.Message.Amount,
            context.Message.Id.ToString(),
            "Невозможно подать предложение для собственного аукциона"
        );

        var bid = new Bid
        {
            Amount = context.Message.Amount,
            AuctionId = context.Message.Id,
            Bidder = context.Message.Bidder
        };

        if (auction.AuctionEnd < DateTime.UtcNow)
        {
            bid.BidStatus = BidStatus.Завершено;
        }
        else
        {
            var highBid = await _dbContext.Bids.Where(p => p.AuctionId == context.Message.Id)
                .OrderByDescending(p => p.Amount).FirstOrDefaultAsync();

            if ((highBid != null && context.Message.Amount > highBid.Amount) || highBid == null)
            {
                bid.BidStatus = BidStatus.Принято;
            }
            if (highBid != null && context.Message.Amount <= highBid.Amount)
            {
                throw new Exception("Ошибка ставки - ставка меньше существующей ставики");
            }
        }
        await _dbContext.Bids.AddAsync(bid);

        await _dbContext.SaveChangesAsync();

        await _publishEndpoint.Publish(new BidPlaced(bid.Id, context.Message.CorrelationId));

        Console.WriteLine($"{DateTime.Now} Получение сообщения - размещена заявка - " +
                 context.Message.Bidder + ", " + context.Message.Amount);
    }
}
