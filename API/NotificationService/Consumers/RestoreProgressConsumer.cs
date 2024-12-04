using System.Reflection;
using System.Text.Json;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Data;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class RestoreProgressConsumer : IConsumer<DataForProcessingServicesList<NotifyItem>>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public RestoreProgressConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<NotifyItem>> context)
    {
        await _hubContext.Clients.Group(context.Message.CallBackType)
        .SendAsync("RestoreProgress", new { message = context.Message.Props });
    }

}
