using Common.Contracts.EventSourcing;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class SetSnapShotFinalConsumer : IConsumer<ESContract>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SetSnapShotFinalConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task Consume(ConsumeContext<ESContract> context)
    {
        await _hubContext.Clients.Group(context.Message.UserLogin)
            .SendAsync("SetSnapShot", context.Message.EventData);
    }
}
