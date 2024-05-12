using AuctionService.Data;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Consumers;

public class AuctionDeletingConsumer : IConsumer<AuctionDeleting>
{
    private readonly AuctionDbContext _auctionDbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionDeletingConsumer(AuctionDbContext auctionDbContext, IPublishEndpoint publishEndpoint)
    {
        _auctionDbContext = auctionDbContext;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<AuctionDeleting> context)
    {
        var auction = await _auctionDbContext.Auctions.FirstOrDefaultAsync(p => p.Id == context.Message.Id);
        if (auction == null)
        {
            throw new Exception("Запись не найдена");
        };
        if (auction.Seller != context.Message.AuctionAuthor)
        {
            throw new Exception("Удалить аукцион может только автор аукциона");
        };
        _auctionDbContext.Auctions.Remove(auction);
        await _auctionDbContext.SaveChangesAsync();
        await _publishEndpoint.Publish(new AuctionDeleted(context.Message.CorrelationId));
        Console.WriteLine("--> Получение сообщения - аукцион удален - " + context.Message.Id);
    }
}
