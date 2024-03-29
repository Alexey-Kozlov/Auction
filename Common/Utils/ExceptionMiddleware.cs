using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.Utils;

public class ExceptionMiddleware
{
    public readonly RequestDelegate _next;
    public readonly ILogger<ExceptionMiddleware> _log;
    public readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> log, IHostEnvironment env)
    {
        _next = next;
        _log = log;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next.Invoke(context);
        }
        catch (Exception e)
        {
            _log.LogError(e, e.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var response = _env.IsDevelopment()
                ? new ApiResponse<string>()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    ErrorMessages = [e.Message, e.StackTrace.ToString()],
                    Result = ""
                }
                : new ApiResponse<string>()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    ErrorMessages = ["Ошибка сервера", "Обратитесь к разработчику"],
                    Result = ""
                };
            var jsonPolicy = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, jsonPolicy);
            await context.Response.WriteAsync(json);
        }
    }

}

