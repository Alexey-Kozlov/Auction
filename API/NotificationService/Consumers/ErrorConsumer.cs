using Common.Contracts.Processing;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class ErrorConsumer : IConsumer<NotificationServiceError>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private string group;

    public ErrorConsumer(IHubContext<NotificationHub> hubContext, IPublishEndpoint publishEndpoint)
    {
        _hubContext = hubContext;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<NotificationServiceError> context)
    {
        context.Message.TraceId = context.Message.TraceId ?? Guid.NewGuid();
        group = string.IsNullOrEmpty(context.Message.UserLogin) ?
            context.Message.SessionId :
            context.Message.UserLogin;
        //посылаем ошибку в UI через SignalR
        if (!string.IsNullOrEmpty(group))
        {
            if (context.Message.IsError)
            {
                await _hubContext.Clients.Group(group).SendAsync("ErrorMessage",
                    new
                    {
                        messageType = 0, //Ошибка
                        auctionId = context.Message.AuctionId ?? null,
                        message = $"Ошибка - Id {context.Message.TraceId}"

                    });
            }
            else
            {
                await _hubContext.Clients.Group(group).SendAsync("ErrorMessage",
                new
                {
                    messageType = 1, //Предупреждение
                    auctionId = context.Message.AuctionId ?? null,
                    message = $"{context.Message.ErrorMessage}"
                });
            }
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
        loggingServiceErrorItem.UserLogin = group;
        loggingServiceErrorItem.IsError = errorItem.IsError;
        loggingServiceErrorItem.TraceId = errorItem.TraceId;
        //посылаем сообщение об ошибке через RabbitMq в LoggingService -> Consumers -> LoggingServiceErrorConsumer
        await _publishEndpoint.Publish(loggingServiceErrorItem);
    }

}
