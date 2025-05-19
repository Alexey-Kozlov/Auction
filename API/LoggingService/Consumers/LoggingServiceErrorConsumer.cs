
using Common.Contracts.Logging;
using Common.Contracts.Processing;
using MassTransit;

namespace Logging.Consumers;

public class LoggingServiceErrorConsumer : IConsumer<LoggingServiceError>
{
    private readonly LoggingConsumer _loggingConsumer;

    public LoggingServiceErrorConsumer(LoggingConsumer loggingConsumer)
    {
        _loggingConsumer = loggingConsumer;
    }

    public async Task Consume(ConsumeContext<LoggingServiceError> context)
    {
        //подписчик логирования через RabbitMq
        var loggingMessage = new ItemLoggingContract
        {
            RequestDate = DateTime.UtcNow,
            UserLogin = context.Message.UserLogin,
            TraceId = null,
            RequestType = context.Message.ErrorServiceName,
            ResponseLoggingContract = new ResponseLoggingContract
            {
                IsSuccess = false,
                ErrorMessages = [context.Message.ErrorServiceName, context.Message.ErrorMessage],
                Result = context.Message.ErrorExceptionMessage,
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            },
            RequestLoggingContract = new RequestLoggingContract
            {
                Body = "",
                Method = "",
                Path = ""
            },
            LogType = LogType.Error
        };
        await _loggingConsumer.WriteLog(loggingMessage);
    }
}