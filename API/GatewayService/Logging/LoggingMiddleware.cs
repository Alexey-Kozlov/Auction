using System.Net;
using System.Text;
using System.Text.Json;
using Common.Contracts.Logging;
using GatewayService.Models;

namespace GatewayService.Logging;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _services;

    public LoggingMiddleware(RequestDelegate next, IServiceProvider services, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
        _services = services;
    }

    public async Task Invoke(HttpContext context)
    {
        //проверяем путь - если нужно исключить запрос из логгирования, или если нет TraceId
        if (ExceptionLoggingItems.CheckPathToInclude($"{context.Request.Path}{DecodeUrlString(context.Request.QueryString.Value)}") ||
            string.IsNullOrEmpty(context.Request.Headers["traceid"]))
        {
            //пропускаем логирование
            await _next(context);
            return;
        }
        var originalBodyStream = context.Response.Body;
        var rezult = new ItemLoggingContract
        {
            RequestDate = DateTime.UtcNow,
            LogType = LogType.Audit
        };
        using (var responseBody = new MemoryStream())
        {
            try
            {
                rezult.RequestLoggingContract = await FormatRequest(context.Request, rezult);
                context.Response.Body = responseBody;
                await _next(context);
                rezult.ResponseLoggingContract = await FormatResponse(context.Response);
                //если в ошибке есть сообщение
                if (!string.IsNullOrEmpty(rezult.ResponseLoggingContract.Result))
                {
                    try
                    {
                        var responseDTO = JsonSerializer.Deserialize<ResponseDTO>(rezult.ResponseLoggingContract.Result);
                        if (responseDTO.statusCode > 399)
                        {
                            rezult.LogType = LogType.Error;
                        }
                    }
                    //пропускаем ошибку десериализации, если в ответе был не json а html
                    catch { }
                }
                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (Exception e)
            {
                rezult.ResponseLoggingContract = new ResponseLoggingContract
                {
                    IsSuccess = false,
                    ErrorMessages = [e.Message, e.StackTrace.ToString()],
                    Result = "",
                    StatusCode = HttpStatusCode.InternalServerError
                };
                rezult.LogType = LogType.Error;
            }
            finally
            {
                using (var scope = _services.CreateScope())
                {
                    var _sendMessage = scope.ServiceProvider.GetRequiredService<SendMessage>();
                    await _sendMessage.SendLogTopic(rezult);
                }
            }
        }
    }

    private async Task<RequestLoggingContract> FormatRequest(HttpRequest request, ItemLoggingContract rezult)
    {
        request.EnableBuffering();
        var buffer = new byte[Convert.ToInt32(request.ContentLength)];
        await request.Body.ReadExactlyAsync(buffer, 0, buffer.Length);
        var bodyAsText = Encoding.UTF8.GetString(buffer);
        var messageLimit = Int32.Parse(_configuration["MaxLogMessageSize"] ?? "10000");
        if (bodyAsText.Length > messageLimit)
        {
            bodyAsText = $"Размер превышает лимит '{messageLimit}' на запись в лог - {bodyAsText.Length}";
        }
        request.Body.Position = 0;
        rezult.TraceId = request.Headers["traceid"];
        rezult.UserLogin = request.Headers["User"];
        rezult.RequestType = request.Headers["RequestType"];
        return new RequestLoggingContract
        {
            Body = bodyAsText,
            Method = request.Method,
            Path = $"{request.Host}{request.Path}{DecodeUrlString(request.QueryString.Value)}"
        };
    }


    private async Task<ResponseLoggingContract> FormatResponse(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        var text = await new StreamReader(response.Body).ReadToEndAsync();
        var messageLimit = Int32.Parse(_configuration["MaxLogMessageSize"] ?? "10000");
        if (text.Length > messageLimit)
        {
            text = $"Размер превышает лимит '{messageLimit}' на запись в лог - {text.Length}";
        }
        response.Body.Seek(0, SeekOrigin.Begin);
        return new ResponseLoggingContract
        {
            Result = text,
            IsSuccess = true,
            ErrorMessages = null,
            StatusCode = HttpStatusCode.OK
        };
    }

    private static string DecodeUrlString(string url)
    {
        string newUrl;
        while ((newUrl = Uri.UnescapeDataString(url)) != url)
        {
            url = newUrl;
        }
        return newUrl;
    }
}