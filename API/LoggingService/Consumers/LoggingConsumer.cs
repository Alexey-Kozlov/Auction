using System.Text.Json;
using Common.Contracts;
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
        //форматирование ошибок
        if (loggingMessage.ResponseLoggingContract.StatusCode == System.Net.HttpStatusCode.OK &&
            !String.IsNullOrEmpty(loggingMessage.ResponseLoggingContract.Result) &&
            loggingMessage.LogType == LogType.Error)
        {
            try
            {
                loggingMessage.ResponseLoggingContract = JsonSerializer.Deserialize<ResponseLoggingContract>(loggingMessage.ResponseLoggingContract.Result,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }
            catch
            {
                var errorMes = JsonSerializer.Deserialize<ApiErrorResponse>(loggingMessage.ResponseLoggingContract.Result,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                loggingMessage.ResponseLoggingContract = new ResponseLoggingContract
                {
                    ErrorMessages = errorMes.ErrorMessages,
                    IsSuccess = errorMes.IsSuccess,
                    StatusCode = errorMes.StatusCode,
                    Result = ""
                };
                loggingMessage.LogType = errorMes.StatusCode == System.Net.HttpStatusCode.Forbidden ?
                    LogType.Audit : LogType.Error;
            }

        }
        await WriteLog(loggingMessage);
    }

    public async Task WriteLog(ItemLoggingContract message)
    {
        //удаляем данные о паролях
        if (message.RequestLoggingContract.Method == "POST" &&
            message.RequestLoggingContract.Path.Contains("identity") &&
            message.RequestLoggingContract.Body.Contains("password"))
        {
            message.RequestLoggingContract.Body = string.Empty;
        }
        //в зависимости от типа сообщения - пишем в разные индексы эластика
        //каждый день - новый индекс
        var todayName = $"{DateTime.Now.Year}_{NumberNormalize(DateTime.Now.Month)}" +
            $"_{NumberNormalize(DateTime.Now.Day)}";
        try
        {
            switch (message.LogType)
            {
                case LogType.Audit:
                    var result = await _client.Client.IndexAsync(message, p => p.Index($"audit_{todayName}"));
                    if (!result.IsValidResponse)
                    {
                        Console.WriteLine(result.DebugInformation);
                    }
                    break;
                case LogType.System:
                    var result1 = await _client.Client.IndexAsync(message, p => p.Index($"system_{todayName}"));
                    if (!result1.IsValidResponse)
                    {
                        Console.WriteLine(result1.DebugInformation);
                    }
                    break;
                case LogType.Error:
                    var result2 = await _client.Client.IndexAsync(message, p => p.Index($"error_{todayName}"));
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

    private string NumberNormalize(int per)
    {
        return per > 9 ? per.ToString() : "0" + per.ToString();
    }
}
