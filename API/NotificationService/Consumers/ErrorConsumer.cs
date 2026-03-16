using Common.Contracts.Processing;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class ErrorConsumer : IConsumer<NotificationServiceError>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly NotificationDbContext _dbContext;
    private List<string> _groups = new List<string>();

    public ErrorConsumer(IHubContext<NotificationHub> hubContext, IPublishEndpoint publishEndpoint,
        NotificationDbContext dbContext)
    {
        _hubContext = hubContext;
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
    }
    public async Task Consume(ConsumeContext<NotificationServiceError> context)
    {
        context.Message.TraceId = context.Message.TraceId.Value;
        var group = context.Message.UserLogin;
        // если сообщение ошибки от системного сервиса (например, завершение аукциона) - 
        // пересылаем администратору системы (если не указан AuctionId) или всем кто подписался
        // на получение уведомлений для данного аукциона (если указан AuctionId)
        if (group == "SystemService" && !context.Message.AuctionId.HasValue)
        {
            _groups.Add("admin");
        }
        else if (group == "SystemService" && context.Message.AuctionId.HasValue)
        {
            var auctionNotifyList = await _dbContext.NotifyItems.Where(p => p.ItemId == context.Message.ItemId.Value).ToListAsync();
            _groups.AddRange(auctionNotifyList.Select(p => p.UserLogin).ToArray());
        }
        else
        {
            _groups.Add(group);
        }
        //посылаем ошибку в UI через SignalR
        if (!string.IsNullOrEmpty(group))
        {
            await _hubContext.Clients.Groups(_groups).SendAsync("ErrorMessage",
                new
                {
                    messageType = 0, //Ошибка
                    auctionId = context.Message.AuctionId ?? null,
                    message = $"{context.Message.ErrorMessage}, Id - {context.Message.TraceId}"

                });
        }

        //пишем ошибку сервисов в лог
        await ErrorLogging(context.Message);
    }

    private async Task ErrorLogging(NotificationServiceError errorItem)
    {
        var loggingServiceErrorItem = new LoggingServiceError();
        loggingServiceErrorItem.AuctionId = errorItem.AuctionId;
        loggingServiceErrorItem.ErrorMessage = errorItem.ErrorMessage;
        loggingServiceErrorItem.ErrorExceptionMessage = errorItem.ErrorExceptionMessage;
        loggingServiceErrorItem.ErrorServiceName = errorItem.ErrorServiceName;
        loggingServiceErrorItem.UserLogin = "SystemService";
        loggingServiceErrorItem.TraceId = errorItem.TraceId;
        loggingServiceErrorItem.ItemId = errorItem.ItemId;
        //посылаем сообщение об ошибке через RabbitMq в LoggingService -> Consumers -> LoggingServiceErrorConsumer
        await _publishEndpoint.Publish(loggingServiceErrorItem);
    }

}
