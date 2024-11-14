using Common.Contracts.EventSourcing;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class RestoreSnapShotDbNotificationConsumer : IConsumer<RestoreSnapShotCompleted>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public RestoreSnapShotDbNotificationConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task Consume(ConsumeContext<RestoreSnapShotCompleted> context)
    {
        Console.WriteLine($"{DateTime.Now} --> Получено сообщение - '{context.Message.Message}'");
        await _hubContext.Clients.Group(context.Message.SessionId)
            .SendAsync("RestoreSnapShotDb", context.Message.Message);
    }
}
