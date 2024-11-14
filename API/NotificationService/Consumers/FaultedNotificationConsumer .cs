using Common.Contracts.Notification;
using Common.Contracts.Processing;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class FaultedNotificationConsumer : IConsumer<FaultMessageSending>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public FaultedNotificationConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task Consume(ConsumeContext<FaultMessageSending> context)
    {
        var message = new FaultNotificationMessage
        (
            context.Message.UserLogin,
            context.Message.Message,
            MessageType.Ошибка,
            null
        );
        await _hubContext.Clients.Group(context.Message.UserLogin).SendAsync("ErrorMessage", message);
    }
}
