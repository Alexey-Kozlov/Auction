using Common.Contracts.Notification;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class ElkindexConsumer : IConsumer<NotificationProgress>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public ElkindexConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;

    }
    public async Task Consume(ConsumeContext<NotificationProgress> context)
    {
        var result = new
        {
            percent = context.Message.Percent,
            duration = context.Message.Duration,
            message = context.Message.Message,
            show = context.Message.Show
        };
        //уведомление при окончании индексации
        await _hubContext.Clients.Group(context.Message.SessionId).SendAsync("ElkIndex", result);
    }
}
