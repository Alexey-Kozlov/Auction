
using Common.Contracts;
using Common.Contracts.Processing;
using Logging.Services;
using MassTransit;

namespace Logging.Consumers;

public class LoggingServiceErrorConsumer : IConsumer<LoggingServiceError>
{
    private readonly ElkClient _client;

    public LoggingServiceErrorConsumer(ElkClient client)
    {
        _client = client;
    }

    public async Task Consume(ConsumeContext<LoggingServiceError> context)
    {
        //подписчик логирования через RabbitMq

        var loggingMessage = new ItemLoggingContract
        {
            RequestDate = DateTime.UtcNow,
            UserLogin = context.Message.UserLogin,
            RequestId = null,
            TraceId = context.Message.TraceId.ToString(),
            RequestType = context.Message.ErrorServiceName,
            ResponseLoggingContract = new ResponseLoggingContract
            {
                IsSuccess = false,
                ErrorMessages = [context.Message.ErrorServiceName, context.Message.ErrorMessage],
                Result = context.Message.ErrorExceptionMessage,
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            }
        };
        await _client.Client.IndexAsync(loggingMessage, p => p.Index("logging_index"));
    }
}