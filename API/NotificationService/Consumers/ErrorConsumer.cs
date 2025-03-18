using System.Reflection;
using Common.Contracts.Processing;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class ErrorConsumer : IConsumer<NotificationServiceError>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public ErrorConsumer(IHubContext<NotificationHub> hubContext,
    IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _hubContext = hubContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<NotificationServiceError> context)
    {
        context.Message.TraceId = Guid.NewGuid();
        //посылаем ошибку в UI через SignalR
        if (context.Message.IsError)
        {
            await _hubContext.Clients.Group(context.Message.UserLogin).SendAsync("ErrorMessage",
                new
                {
                    messageType = 0, //Ошибка
                    auctionId = context.Message.AuctionId,
                    message = $"Ошибка - Id {context.Message.TraceId}"

                });
        }
        else
        {
            await _hubContext.Clients.Group(context.Message.UserLogin).SendAsync("ErrorMessage",
            new
            {
                messageType = 1, //Предупреждение
                auctionId = context.Message.AuctionId,
                message = $"{context.Message.Message}"
            });
        }

        //пишем ошибку сервисов в лог
        await ErrorLogging(context.Message);

        if (!string.IsNullOrEmpty(context.Message.CallBackType))
        {
            var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
            //продолжаем обработку ProcessingService
            await _publishEndpoint.Publish(sendObject);
        }
    }

    private async Task ErrorLogging(NotificationServiceError errorItem)
    {
        var loggingServiceErrorItem = new LoggingServiceError();
        loggingServiceErrorItem.AuctionId = errorItem.AuctionId;
        loggingServiceErrorItem.Message = errorItem.Message;
        loggingServiceErrorItem.ExceptionMessage = errorItem.ExceptionMessage;
        loggingServiceErrorItem.ServiceName = errorItem.ServiceName;
        loggingServiceErrorItem.UserLogin = errorItem.UserLogin;
        loggingServiceErrorItem.IsError = errorItem.IsError;
        loggingServiceErrorItem.TraceId = errorItem.TraceId;
        //посылаем сообщение об ошибке через RabbitMq в LoggingService -> Consumers -> LoggingServiceErrorConsumer
        await _publishEndpoint.Publish(loggingServiceErrorItem);
    }

}
