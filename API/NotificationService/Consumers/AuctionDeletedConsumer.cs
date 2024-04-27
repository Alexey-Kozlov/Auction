using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
{
    private readonly NotificationDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public AuctionDeletedConsumer(NotificationDbContext context, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }
    public async Task Consume(ConsumeContext<AuctionDeleted> consumeContext)
    {
        Console.WriteLine("--> Получение сообщения удалить аукцион, пользователь - " + consumeContext.Message.AuctionAuthor);
        var items = await _context.NotifyUser.Where(p => p.AuctionId == consumeContext.Message.Id).ToListAsync();
        if (items != null && items.Count > 0)
        {
            _context.NotifyUser.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
        await _hubContext.Clients.Group(consumeContext.Message.AuctionAuthor).SendAsync("AuctionDeleted", consumeContext.Message);
    }
}
