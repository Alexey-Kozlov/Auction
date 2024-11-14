using Common.Contracts.EventSourcing;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class SetSnapShotDbNotificationConsumer : IConsumer<EventSourcingInitialized>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SetSnapShotDbNotificationConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task Consume(ConsumeContext<EventSourcingInitialized> context)
    {
        Console.WriteLine($"{DateTime.Now} --> Получено сообщение - '{context.Message.Message}'");
        await _hubContext.Clients.Group(context.Message.SessionId)
            .SendAsync("SetSnapShotDb", context.Message.Message);
    }
}
