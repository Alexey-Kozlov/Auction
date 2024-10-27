using Common.Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class AuctionUpdatingNotificationConsumer : IConsumer<AuctionUpdatingNotification>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionUpdatingNotificationConsumer(IHubContext<NotificationHub> hubContext, IPublishEndpoint publishEndpoint)
    {
        _hubContext = hubContext;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<AuctionUpdatingNotification> context)
    {
        await _hubContext.Clients.Group(context.Message.UserLogin).SendAsync("AuctionUpdated", context.Message);
        await _publishEndpoint.Publish(new AuctionUpdatedNotification(context.Message.CorrelationId));
    }
}
