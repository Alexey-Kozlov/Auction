using AuctionService.Data;
using Common.Contracts;
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
        var auction = await _auctionDbContext.Auctions.FindAsync(context.Message.Id);
        if (context.Message.Amount > auction.CurrentHighBid || auction.CurrentHighBid == 0)
        {
            var oldBid = auction.CurrentHighBid; ;
            auction.CurrentHighBid = context.Message.Amount;
            await _auctionDbContext.SaveChangesAsync();
            Console.WriteLine("--> Получение сообщения - размещена заявка, AuctionId - " + context.Message.Id + ", ставка - "
                + context.Message.Amount);
            await _publishEndpoint.Publish(new BidAuctionPlaced(oldBid, context.Message.CorrelationId));
            return;
        }
        throw new Exception("Ошибка BidAuctionPlacingConsumer");
    }
}
