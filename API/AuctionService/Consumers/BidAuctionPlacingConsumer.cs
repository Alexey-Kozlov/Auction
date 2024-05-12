using AuctionService.Data;
using AuctionService.Exceptions;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class BidAuctionPlacingConsumer : IConsumer<BidAuctionPlacing>
{
    private readonly AuctionDbContext _auctionDbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public BidAuctionPlacingConsumer(AuctionDbContext auctionDbContext, IPublishEndpoint publishEndpoint)
    {
        _auctionDbContext = auctionDbContext;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<BidAuctionPlacing> context)
    {
        var auction = await _auctionDbContext.Auctions.FindAsync(context.Message.AuctionId);
        if (context.Message.Amount > auction.CurrentHighBid || auction.CurrentHighBid == 0)
        {
            auction.CurrentHighBid = context.Message.Amount;
            await _auctionDbContext.SaveChangesAsync();
            Console.WriteLine("--> Получение сообщения - размещена заявка, AuctionId - " + context.Message.AuctionId + ", ставка - "
                + context.Message.Amount);
            await _publishEndpoint.Publish(new BidAuctionPlaced(auction.CurrentHighBid, context.Message.CorrelationId));
            return;
        }
        throw new BidAuctionPlacingException(context.Message.Bidder, context.Message.Amount,
            context.Message.AuctionId.ToString(), "Ошибка BidAuctionPlacingConsumer");
    }
}
