using AuctionService.Data;
using AuctionService.Metrics;
using Common.Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class BidAuctionPlacingConsumer : IConsumer<BidAuctionPlacing>
{
    private readonly AuctionDbContext _auctionDbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly AuctionMetrics _metrics;

    public BidAuctionPlacingConsumer(AuctionDbContext auctionDbContext, IPublishEndpoint publishEndpoint, AuctionMetrics metrics)
    {
        _auctionDbContext = auctionDbContext;
        _publishEndpoint = publishEndpoint;
        _metrics = metrics;
    }
    public async Task Consume(ConsumeContext<BidAuctionPlacing> context)
    {
        var auction = await _auctionDbContext.Auctions.FindAsync(context.Message.Id);
        if (context.Message.Amount > auction.CurrentHighBid || auction.CurrentHighBid == 0)
        {
            var oldBid = auction.CurrentHighBid; ;
            auction.CurrentHighBid = context.Message.Amount;
            await _auctionDbContext.SaveChangesAsync();
            _metrics.BidAuction();
            Console.WriteLine($"{DateTime.Now}--> Получение сообщения - размещена заявка, AuctionId - {context.Message.Id}, ставка - {context.Message.Amount}");
            await _publishEndpoint.Publish(new BidAuctionPlaced(oldBid, context.Message.CorrelationId));
            return;
        }
        else
        {
            throw new Exception($"{DateTime.Now} - Ошибка BidAuctionPlacingConsumer, новая ставка меньше или равна текущей.");
        }
    }
}
