using Common.Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Data;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class SetSnapShotDbNotificationConsumer : IConsumer<EventSourcingInitialized>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly NotificationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public SetSnapShotDbNotificationConsumer(IHubContext<NotificationHub> hubContext,
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
            .SendAsync("SetSnapShotDb", context.Message.Message);
    }
}
