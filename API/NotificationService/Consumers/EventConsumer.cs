using System.Reflection;
using Common.Contracts.Processing;
using MassTransit;
using MassTransit.Initializers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class EventConsumer : IConsumer<EventNotificationItem>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly NotificationDbContext _dbContext;

    public EventConsumer(IHubContext<NotificationHub> hubContext, NotificationDbContext dbContext)
    {
        _hubContext = hubContext;
        _dbContext = dbContext; ;
    }
    public async Task Consume(ConsumeContext<EventNotificationItem> context)
    {
        var notifyList = new List<string>();
        if (context.Message.AuctionId.HasValue)
        {
            notifyList.AddRange(await _dbContext.NotifyItems.Where(p =>
                p.AuctionId == context.Message.AuctionId && p.Commited)
                .Select(p => p.UserLogin).ToListAsync()
            );
        }
        // если указан и SessionId и UserLogin - может быть дублированная рассылка по одному адресу,
        // здесь избегаем дублей.
        if (!string.IsNullOrEmpty(context.Message.SessionId) &&
            string.IsNullOrEmpty(context.Message.UserLogin))
        {
            notifyList.Add(context.Message.SessionId);
        }

        await _hubContext.Clients.Groups(notifyList.Select(p => p))
            .SendAsync(Enum.GetName(typeof(SignalRMethod), context.Message.SignalRMethod),
            new
            {
                show = context.Message.Show,
                data = context.Message.Data
            });
    }
}
