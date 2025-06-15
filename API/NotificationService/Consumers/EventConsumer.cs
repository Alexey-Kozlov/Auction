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

        switch (context.Message.SessionId)
        {
            //рассылка по всем пользователям
            case "all":
                await _hubContext.Clients.All
                    .SendAsync(Enum.GetName(typeof(SignalRMethod), context.Message.SignalRMethod),
                    new
                    {
                        show = context.Message.Show,
                        data = context.Message.Data
                    });
                break;
            //рассылка по подписчикам на данный аукцион
            case "auctionGroup":
                await _hubContext.Clients.Groups(await _dbContext.NotifyItems.Where(p =>
                    p.AuctionId == context.Message.AuctionId && p.Commited)
                    .Select(p => p.UserLogin).ToListAsync())
                    .SendAsync(Enum.GetName(typeof(SignalRMethod), context.Message.SignalRMethod),
                    new
                    {
                        show = context.Message.Show,
                        data = context.Message.Data
                    });
                break;
            //здесь указано значение SessionId пользователя, рассылка только этому пользователю            
            default:
                await _hubContext.Clients.Groups(context.Message.SessionId)
                    .SendAsync(Enum.GetName(typeof(SignalRMethod), context.Message.SignalRMethod),
                    new
                    {
                        show = context.Message.Show,
                        data = context.Message.Data
                    });
                break;
        }

        if (context.Message.AuctionId.HasValue)
        {
            notifyList.AddRange(await _dbContext.NotifyItems.Where(p =>
                p.AuctionId == context.Message.AuctionId && p.Commited)
                .Select(p => p.UserLogin).ToListAsync()
            );
        }
        // если указан SessionId и нет этого пользователя в выборке извещений - 
        // добавляем SessionId для передачи уведомления текущему пользователю
        // if (!string.IsNullOrEmpty(context.Message.SessionId) &&
        //     !notifyList.Any(p => p == context.Message.UserLogin))
        // {
        //     notifyList.Add(context.Message.SessionId);
        // }

        await _hubContext.Clients.Groups(notifyList.Select(p => p))
            .SendAsync(Enum.GetName(typeof(SignalRMethod), context.Message.SignalRMethod),
            new
            {
                show = context.Message.Show,
                data = context.Message.Data
            });
    }
}
