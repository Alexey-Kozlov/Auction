using Common.Contracts.Notification;
using Common.Contracts.Processing;
using Google.Protobuf.WellKnownTypes;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class SetsnapshotProgressConsumer : IConsumer<DataForProcessingServicesList<NotifyItem>>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SetsnapshotProgressConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<NotifyItem>> context)
    {
        var result = new
        {
            percent = context.Message.Props.Split(';')[0],
            duration = bool.Parse(context.Message.Props.Split(';')[1]) ? 5000 : 20000
        };
        await _hubContext.Clients.Group(context.Message.CallBackType)
        .SendAsync("SetSnapShotProgress", result);
    }

}
