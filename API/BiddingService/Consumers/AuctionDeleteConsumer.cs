using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using BiddingService.Data;

namespace BiddingService.Consumers;

public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
{
    private readonly BidDbContext _context;

    public AuctionDeletedConsumer(BidDbContext context)
    {
        _context = context;
    }
    public async Task Consume(ConsumeContext<AuctionDeleted> consumeContext)
    {
        Console.WriteLine("--> Получение сообщения удалить аукцион");
        var bids = await _context.Bids.Where(p => p.AuctionId == consumeContext.Message.Id).ToListAsync();
        if (bids != null && bids.Count > 0)
        {
            _context.Bids.RemoveRange(bids);
        }
        var auction = await _context.Auctions.FirstOrDefaultAsync(p => p.Id == consumeContext.Message.Id);
        if (auction != null)
        {
            _context.Auctions.Remove(auction);
        }
        await _context.SaveChangesAsync();
    }
}
