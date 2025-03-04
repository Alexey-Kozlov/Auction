public static class LoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestResponseLogging1(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<LoggingMiddleware1>();
    }
}