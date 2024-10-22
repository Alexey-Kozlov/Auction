using Common.Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class ElkIndexCreatingNotificationConsumer : IConsumer<ElkIndexResponse>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public ElkIndexCreatingNotificationConsumer(IHubContext<NotificationHub> hubContext,
        IPublishEndpoint publishEndpoint)
    {
        _hubContext = hubContext;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<ElkIndexResponse> context)
    {
        Console.WriteLine($"{DateTime.Now} --> Получено сообщение - проиндексирована запись, последняя -  {context.Message.LastItem}");
        if (context.Message.LastItem)
        {
            await _hubContext.Clients.Group(context.Message.SessionId).SendAsync("ElkIndex",
                $"Проиндексировано - {context.Message.ItemNumber} записей");
        }
        await _publishEndpoint.Publish(new ElkIndexResponseCompleted(context.Message.CorrelationId));
    }
}
