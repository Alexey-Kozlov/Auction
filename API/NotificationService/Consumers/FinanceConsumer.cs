using System.Reflection;
using System.Text.Json;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class FinanceConsumer : IConsumer<DataForProcessingServicesList<NotifyItem>>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public FinanceConsumer(IHubContext<NotificationHub> hubContext,
        IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _hubContext = hubContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<NotifyItem>> context)
    {
        var correlationId = context.Message.CorrelationId;
        var amount = context.Message.Props.Split(",")[0];
        var show = context.Message.Props.Split(",")[1].ToLower();
        foreach (var item in context.Message.DataObjects)
        {
            //уведомление при операции поступлении денег на счет
            var typedItem = JsonSerializer.Deserialize<NotifyItem>(item.Data);
            await _hubContext.Clients.Group(typedItem.UserLogin).SendAsync("FinanceCreate",
                new { value = amount, show = show });
        }

        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
        await _publishEndpoint.Publish(sendObject);
    }
}
