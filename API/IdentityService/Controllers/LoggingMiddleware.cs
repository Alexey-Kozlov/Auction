using System.Net;
using System.Text;
using System.Text.Json;
using Common.Contracts;

public class LoggingMiddleware1
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;
    public readonly IHostEnvironment _env;
    private readonly IConfiguration _configuration;

    public LoggingMiddleware1(RequestDelegate next, ILoggerFactory loggerFactory, IHostEnvironment env,
    IConfiguration configuration)
    {
        _next = next;
        _logger = loggerFactory.CreateLogger<LoggingMiddleware1>();
        _env = env;
        _configuration = configuration;
    }

    public async Task Invoke(HttpContext context)
    {
        _logger.LogInformation(await FormatRequest(context.Request));

        var originalBodyStream = context.Response.Body;

        using (var responseBody = new MemoryStream())
        {
            try
            {
                context.Response.Body = responseBody;
                await _next(context);
                _logger.LogInformation(await FormatResponse(context.Response));
                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (Exception e)
            {
                await ExceptionMiddleware(e, context);
            }
        }
    }

    private async Task<string> FormatRequest(HttpRequest request)
    {
        request.EnableBuffering();
        var buffer = new byte[Convert.ToInt32(request.ContentLength)];
        await request.Body.ReadExactlyAsync(buffer, 0, buffer.Length);
        var bodyAsText = Encoding.UTF8.GetString(buffer);
        var messageLimit = Int32.Parse(_configuration["MaxLogMessageSize"] ?? "10000");
        if (bodyAsText.Length > messageLimit)
        {
            bodyAsText = "Request превышает лимит на запись в лог - " + messageLimit;
        }
        request.Body.Position = 0;
        return $"{request.Scheme} {request.Host}{request.Path} {DecodeUrlString(request.QueryString.Value)} {bodyAsText}";
    }


    private async Task<string> FormatResponse(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        var text = await new StreamReader(response.Body).ReadToEndAsync();
        var messageLimit = Int32.Parse(_configuration["MaxLogMessageSize"] ?? "10000");
        if (text.Length > messageLimit)
        {
            text = "Response превышает лимит на запись в лог - " + messageLimit;
        }
        response.Body.Seek(0, SeekOrigin.Begin);
        return $"Response {text}";
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

    private async Task ExceptionMiddleware(Exception e, HttpContext context)
    {
        _logger.LogError(e, e.Message);
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