using AuctionService.Data;
using AuctionService.Entities;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionFinishingConsumer : IConsumer<AuctionFinishing>
{
    private readonly AuctionDbContext _auctionDbContext;

    public AuctionFinishingConsumer(AuctionDbContext auctionDbContext)
    {
        _auctionDbContext = auctionDbContext;
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

        Console.WriteLine("--> Получение сообщения - аукцион завершен - " + context.Message.Id);
    }
}
