using AuctionService.Data;
using Common.Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class RollbackBidAuctionPlacedConsumer : IConsumer<RollbackBidAuctionPlaced>
{
    private readonly AuctionDbContext _auctionDbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public RollbackBidAuctionPlacedConsumer(AuctionDbContext auctionDbContext, IPublishEndpoint publishEndpoint)
    {
        _auctionDbContext = auctionDbContext;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<RollbackBidAuctionPlaced> context)
    {
        var auction = await _auctionDbContext.Auctions.FindAsync(context.Message.Id);
        auction.CurrentHighBid = context.Message.OldHighBid;
        await _auctionDbContext.SaveChangesAsync();
        Console.WriteLine($"{DateTime.Now} --> Получение сообщения - восстановление старого значения максимальной ставки" +
        $" вследствие отмены последней ставки, AuctionId - {context.Message.Id}, старая ставка - " +
        $" {context.Message.OldHighBid}");
    }
}
