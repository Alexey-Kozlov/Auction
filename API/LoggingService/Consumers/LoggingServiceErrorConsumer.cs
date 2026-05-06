
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
            TraceId = context.Message.TraceId.HasValue ? context.Message.TraceId.Value.ToString() : "",
            RequestType = context.Message.ErrorServiceName,
            ResponseLoggingContract = new ResponseLoggingContract
            {
                IsSuccess = false,
                ErrorMessages = [context.Message.ErrorMessage],
                ServiceName = context.Message.ErrorServiceName,
                Result = context.Message.ErrorExceptionStack,
                StatusCode = System.Net.HttpStatusCode.InternalServerError,
                InputData = context.Message.ErrorExceptionInputData
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