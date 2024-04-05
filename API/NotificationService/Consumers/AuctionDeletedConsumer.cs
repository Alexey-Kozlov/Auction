using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;

namespace NotificationService.Consumers;

public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
{
    private readonly NotificationDbContext _context;

    public AuctionDeletedConsumer(NotificationDbContext context)
    {
        _context = context;
    }
    public async Task Consume(ConsumeContext<AuctionDeleted> consumeContext)
    {
        Console.WriteLine("--> Получение сообщения удалить аукцион");
        var items = await _context.NotifyUser.Where(p => p.AuctionId == consumeContext.Message.Id).ToListAsync();
        if (items != null && items.Count > 0)
        {
            _context.NotifyUser.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
    }
}
