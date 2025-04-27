using System.Text.Json;
using Common.Contracts.Logging;
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
        //форматирование ошибок
        if (loggingMessage.ResponseLoggingContract.StatusCode == System.Net.HttpStatusCode.OK &&
            loggingMessage.ResponseLoggingContract.Result != null &&
            loggingMessage.LogType == LogType.Error)
        {
            loggingMessage.ResponseLoggingContract = JsonSerializer.Deserialize<ResponseLoggingContract>(loggingMessage.ResponseLoggingContract.Result,
            jsonPolicy);
        }
        await WriteLog(loggingMessage);
    }

    public async Task WriteLog(ItemLoggingContract message)
    {
        //в зависимости от типа сообщения - пишем в разные индексы эластика
        //каждый день - новый индекс
        var todayName = $"{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}";
        try
        {
            switch (message.LogType)
            {
                case LogType.Audit:
                    var result = await _client.Client.IndexAsync(message, p => p.Index($"audit-{todayName}"));
                    if (!result.IsValidResponse)
                    {
                        Console.WriteLine(result.DebugInformation);
                    }
                    break;
                case LogType.System:
                    var result1 = await _client.Client.IndexAsync(message, p => p.Index($"system-{todayName}"));
                    if (!result1.IsValidResponse)
                    {
                        Console.WriteLine(result1.DebugInformation);
                    }
                    break;
                case LogType.Error:
                    var result2 = await _client.Client.IndexAsync(message, p => p.Index($"error-{todayName}"));
                    if (!result2.IsValidResponse)
                    {
                        Console.WriteLine(result2.DebugInformation);
                    }
                    break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Write logging error - {e.Message}");
        }

    }
}
