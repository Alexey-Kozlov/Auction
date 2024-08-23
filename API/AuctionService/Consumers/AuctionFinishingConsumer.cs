using AuctionService.Data;
using AuctionService.Entities;
using AuctionService.Metrics;
using Common.Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionFinishingConsumer : IConsumer<AuctionFinishing>
{
    private readonly AuctionDbContext _auctionDbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly AuctionMetrics _metrics;

    public AuctionFinishingConsumer(AuctionDbContext auctionDbContext, IPublishEndpoint publishEndpoint, AuctionMetrics metrics)
    {
        _auctionDbContext = auctionDbContext;
        _publishEndpoint = publishEndpoint;
        _metrics = metrics;
    }
    public async Task Consume(ConsumeContext<AuctionFinishing> context)
    {
        var auction = await _auctionDbContext.Auctions.FindAsync(context.Message.Id);
        if (context.Message.ItemSold)
        {
            auction.Winner = context.Message.Winner;
            auction.SoldAmount = context.Message.Amount;
        }
        auction.Status = auction.SoldAmount > auction.ReservePrice ? Status.Закончен : Status.НеПродано;
        await _auctionDbContext.SaveChangesAsync();
        await _publishEndpoint.Publish(new AuctionFinished(context.Message.CorrelationId));
        _metrics.FinishAuction();
        Console.WriteLine("--> Получение сообщения - аукцион завершен - " + context.Message.Id);
    }
}
