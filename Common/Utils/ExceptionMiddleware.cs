using System.Net;
using System.Text.Json;
using Common.Contracts;
using Common.Utils.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.Utils;

public class ExceptionMiddleware
{
    public readonly RequestDelegate _next;
    public readonly ILogger<ExceptionMiddleware> _log;
    public readonly IHostEnvironment _env;
    private readonly IsAdminModeService _isAdminModeService;
    private readonly IConfiguration _config;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> log, IHostEnvironment env,
        IsAdminModeService isAdminModeService, IConfiguration config)
    {
        _next = next;
        _log = log;
        _env = env;
        _isAdminModeService = isAdminModeService;
        _config = config;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            //можно и так - через создание scope и вызов в нем сервиса, но в нашем случае у нас уже scope
            //создан (в рамках запроса в конвейере), так что напрямую вызываем серис через конструктор как обычно

            //public async Task InvokeAsync(HttpContext context, IServiceProvider services){
            //var srv = services.CreateScope().ServiceProvider.GetRequiredService<IsAdminModeService>();
            //var dd = srv.GetAdminMode();}

            var skipCheck = Boolean.Parse(_config["SkipCheckSettings"]);
            //Если нет ответа от сервиса Settings, ждем пока заработает
            if (!skipCheck && !_isAdminModeService.GetAdminMode().CheckResult)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 400; //Badrequest
                ApiResponse<string> value = new ApiResponse<string>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = new List<string>(2)
                {
                    "SystemMessage",
                    "В системе запускаются сервисы..."
                },
                    Result = ""
                };
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                await context.Response.WriteAsync(JsonSerializer.Serialize(value, options));
                _log.LogError("SystemMessage - В системе запускаются сервисы...");
                return;
            }
            //Также если система находится в "AdminMode" - для всех пользователей, кроме админов - запретить прохождение
            //запроса по конвейеру, выдать http-ответ - "Система в режиме обслуживания"
            if (!skipCheck && _isAdminModeService.GetAdminMode().AdminMode && !context.User.IsInRole("Admin"))
            {
                //запрет доступа
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 400; //Badrequest
                ApiResponse<string> value = new ApiResponse<string>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = new List<string>(2)
                {
                    "SystemMessage",
                    "Система на обслуживании."
                },
                    Result = ""
                };
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                await context.Response.WriteAsync(JsonSerializer.Serialize(value, options));
                _log.LogError("SystemMessage - Система на обслуживании.");
                return;
            }
            await _next(context);
        }
        catch (Exception e)
        {
            _log.LogError(e, e.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var response = new ApiResponse<string>()
            {
                StatusCode = HttpStatusCode.InternalServerError,
                IsSuccess = false,
                ErrorMessages = [e.Message, e.StackTrace.ToString()],
                Result = ""
            };
            var jsonPolicy = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, jsonPolicy);
            await context.Response.WriteAsync(json);
        }
    }

}

