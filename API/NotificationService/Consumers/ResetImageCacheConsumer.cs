using Common.Contracts.Image;
using Common.Contracts.Processing;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class ResetImageCacheConsumer : IConsumer<ResetImageCacheNotification>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public ResetImageCacheConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task Consume(ConsumeContext<ResetImageCacheNotification> context)
    {
        await _hubContext.Clients.Group(context.Message.SessionId).SendAsync(
            Enum.GetName(typeof(SignalRMethod), SignalRMethod.ResetImageCache),
        "Выполнен сброс кеша изображений Redis");
    }
}
