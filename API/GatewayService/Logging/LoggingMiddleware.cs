using System.Security.Claims;
using System.Text;
using Common.Contracts;
using Microsoft.AspNetCore.Mvc;

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
        var originalBodyStream = context.Response.Body;
        var rezult = new ItemLoggingContract();
        rezult.UserLogin = context.User?.Claims.Where(p => p.Type == "Login").Select(p => p.Value).FirstOrDefault();
        rezult.Roles = context.User?.Claims.Where(p => p.Type == ClaimTypes.Role).Select(p => p.Value).FirstOrDefault();
        rezult.RequestDate = DateTime.UtcNow;
        using (var responseBody = new MemoryStream())
        {
            try
            {
                rezult.RequestLoggingContract = await FormatRequest(context.Request, rezult);
                context.Response.Body = responseBody;
                await _next(context);
                rezult.ResponseLoggingContract = await FormatResponse(context.Response);
                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (Exception e)
            {
                rezult.ResponseLoggingContract = new ResponseLoggingContract
                {
                    Body = $"Error - {e.Message}, {e.StackTrace}",
                    StatusCode = 500
                };
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
        rezult.RequestId = request.Headers["requestid"];
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
            Body = text,
            StatusCode = 200
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