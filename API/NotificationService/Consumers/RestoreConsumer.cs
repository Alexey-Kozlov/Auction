using System.Reflection;
using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class RestoreConsumer : IConsumer<DataForProcessingServicesList<NotifyItem>>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly NotificationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public RestoreConsumer(IHubContext<NotificationHub> hubContext,
    NotificationDbContext dbContext, IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _hubContext = hubContext;
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<NotifyItem>> context)
    {
        var correlationId = context.Message.CorrelationId;
        var mes = !string.IsNullOrEmpty(context.Message.Props) ? context.Message.Props : "";
        foreach (var item in context.Message.DataObjects)
        {
            //восстановление данных из ES лога - восстанавливаем все записи уведомлений и посылаем
            //уведомление о завершении работы
            var typedItem = JsonSerializer.Deserialize<NotifyItem>(item.Data);
            await _dbContext.NotifyItems.AddAsync(typedItem);
        }

        await _dbContext.SaveChangesAsync();
        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
        await _publishEndpoint.Publish(sendObject);
        await _hubContext.Clients.All.SendAsync("RestoreSnapShot", new { message = mes });
    }

}
