
using MassTransit;
using MetricsService.Consumers;
using MetricsService.Metrics;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithMetrics(opt => opt
    .AddAspNetCoreInstrumentation()
    .AddRuntimeInstrumentation()
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("AuctionStoreMetrics"))
    .AddMeter(builder.Configuration.GetValue<string>("AuctionStoreMeterName"))

//.AddPrometheusExporter());  <- это если хотим экспортировать напрямую в Prometheus
//тогда в настройках Prometheus установить на подключение по хосту - host.docker.internal:7008


//.AddConsoleExporter()); <- это для обычного вывода в консоль
.AddOtlpExporter(options =>
{
    //будет вывод на порт 4317 по протоколу grpc
    options.Endpoint = new Uri(builder.Configuration["Otlp:Endpoint"] ?? throw new InvalidOperationException());
}));

builder.Services.AddSingleton<AuctionMetrics>();

builder.Services.AddMassTransit(p =>
{
    p.AddConsumersFromNamespaceContaining<AuctionCreatingMetricsConsumer>();
    p.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("metrics", false));
    p.UsingRabbitMq((context, config) =>
    {
        config.Host(builder.Configuration["RabbitMq:Host"], "/", p =>
        {
            p.Username(builder.Configuration.GetValue("RabbitMq:UserName", "guest"));
            p.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });
        config.ConfigureEndpoints(context);
    });
});

var app = builder.Build();
app.Use(async (context, next) =>
{
    //логируем вошедший запрос
    Console.WriteLine($"{DateTime.Now} Вошедший запрос -> {context.Request.Path}");
    await next.Invoke();
    // Do logging or other work that doesn't write to the Response.
});
//app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.Run();
