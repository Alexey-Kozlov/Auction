using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;

namespace SearchService.Consumers;

public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
{
    private readonly SearchDbContext _context;

    public AuctionDeletedConsumer(SearchDbContext context)
    {
        _context = context;
    }
    public async Task Consume(ConsumeContext<AuctionDeleted> consumeContext)
    {
        Console.WriteLine("--> Получение сообщения удалить аукцион");
        var item = await _context.Items.FirstOrDefaultAsync(p => p.Id == consumeContext.Message.Id);
        if (item != null)
        {
            _context.Items.Remove(item);
            //await Task.Delay(TimeSpan.FromSeconds(5));
            await _context.SaveChangesAsync();
            return;
        }
    }
}
