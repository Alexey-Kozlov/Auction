using System.Text.Json;
using Common.Contracts;
using Logging.Services;
using MassTransit;

namespace Logging.Consumers;

public class LoggingConsumer : IConsumer<ItemLoggingContract>
{
    private readonly ElkClient _client;

    public LoggingConsumer(ElkClient client)
    {
        _client = client;
    }
    public async Task Consume(ConsumeContext<ItemLoggingContract> context)
    {
        //подписчик логирования через Kafka
        var loggingMessage = context.Message;
        var jsonPolicy = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        if (loggingMessage.ResponseLoggingContract.StatusCode == System.Net.HttpStatusCode.OK &&
        loggingMessage.ResponseLoggingContract.Result != null &&
            loggingMessage.ResponseLoggingContract.Result.Contains("\"isSuccess\":false"))
        {
            loggingMessage.ResponseLoggingContract = JsonSerializer.Deserialize<ResponseLoggingContract>(loggingMessage.ResponseLoggingContract.Result,
            jsonPolicy);
        }
        await _client.Client.IndexAsync(loggingMessage, p => p.Index("logging_index"));
    }
}
