using Common.Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class AuctionDeletingNotificationConsumer : IConsumer<AuctionDeletingNotification>
{
    private readonly NotificationDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionDeletingNotificationConsumer(NotificationDbContext context,
    IHubContext<NotificationHub> hubContext, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _hubContext = hubContext;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<AuctionDeletingNotification> context)
    {
        Console.WriteLine($"{DateTime.Now} --> Получено сообщение - удалить аукцион '{context.Message.Id}'" +
            $", автор - {context.Message.AuctionAuthor}");
        var items = await _context.NotifyUser.Where(p => p.AuctionId == context.Message.Id).ToListAsync();
        if (items != null && items.Count > 0)
        {
            _context.NotifyUser.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
        await _hubContext.Clients.Group(context.Message.AuctionAuthor).SendAsync("AuctionDeleted", context.Message);
        await _publishEndpoint.Publish(new AuctionDeletedNotification(context.Message.CorrelationId));
    }
}
