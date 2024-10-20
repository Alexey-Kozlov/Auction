using Common.Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Data;
using NotificationService.Entities;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class EventSourcingInitializeNotificationConsumer : IConsumer<EventSourcingInitialized>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly NotificationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public EventSourcingInitializeNotificationConsumer(IHubContext<NotificationHub> hubContext,
    NotificationDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _hubContext = hubContext;
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<EventSourcingInitialized> context)
    {
        Console.WriteLine($"{DateTime.Now} --> Получено сообщение - '{context.Message.Message}'");
        await _hubContext.Clients.Group(context.Message.SessionId)
            .SendAsync("ElkIndex", context.Message.Message);
    }
}
