using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;

namespace SearchService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    private readonly SearchDbContext _context;

    public BidPlacedConsumer(SearchDbContext context)
    {
        _context = context;
    }
    public async Task Consume(ConsumeContext<BidPlaced> consumerContext)
    {
        Console.WriteLine("--> Получение сообщения разместить заявку");

        var auction = await _context.Items.FirstOrDefaultAsync(p => p.Id == consumerContext.Message.AuctionId);
        if (consumerContext.Message.Amount > auction.CurrentHighBid)
        {
            auction.CurrentHighBid = consumerContext.Message.Amount;
            await _context.SaveChangesAsync();
        }
    }
}
