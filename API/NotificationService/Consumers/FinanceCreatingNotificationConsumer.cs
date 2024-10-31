using Common.Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class FinanceCreatingNotificationConsumer : IConsumer<FinanceCreatingNotification>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public FinanceCreatingNotificationConsumer(IHubContext<NotificationHub> hubContext,
        IPublishEndpoint publishEndpoint)
    {
        _hubContext = hubContext;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<FinanceCreatingNotification> context)
    {
        await _hubContext.Clients.Group(context.Message.UserLogin).SendAsync("FinanceCreate", context.Message);
        await _publishEndpoint.Publish(new FinanceNotificationCreated(context.Message.CorrelationId));
    }
}
