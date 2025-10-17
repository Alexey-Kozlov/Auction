using Common.Utils.Logging;
using Common.Utils.Vault;
using ElasticSearchService.Consumers;
using ElasticSearchService.Services;
using ElasticSearchService.Services.Search;
using MassTransit;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddVault(options =>
{
    var vaultOptions = builder.Configuration.GetSection("Vault");
    options.Address = vaultOptions["Address"];
    options.Role = vaultOptions["VAULT_ROLE_ID"];
    options.SecretPathRt = vaultOptions["SecretPathRt"];
    options.SecretPathElk = vaultOptions["SecretPathElk"];
    options.SecretPathRedis = vaultOptions["SecretPathRedis"];
    options.Secret = vaultOptions["VAULT_SECRET_ID"];
});
builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddMassTransit(p =>
{
    p.AddConsumersFromNamespaceContaining<ElkConsumer>();
    p.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("elk", false));
    p.UsingRabbitMq((context, config) =>
    {
        config.Host(builder.Configuration["rt:host"], "/", p =>
        {
            p.Username(builder.Configuration["rt:username"]);
            p.Password(builder.Configuration["rt:password"]);
        });
        config.ConfigureEndpoints(context);
        config.ConcurrentMessageLimit = 1;
    });
});

builder.Services.AddScoped<ElkClient>();
builder.Services.AddScoped<GetSearchItems>();
builder.Services.AddScoped<SearchElk>();
builder.Services.AddResourceMonitoring();
builder.Services.AddOpenTelemetry().WithMetrics(opt => opt
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("MetricGroup")))
    .AddProcessInstrumentation()
    .AddAspNetCoreInstrumentation()
    .AddMeter("Microsoft.Extensions.Diagnostics.ResourceMonitoring")
    .AddPrometheusExporter()
);
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["rd:config"];
    options.InstanceName = builder.Configuration["rd:instance"];
});
builder.Services.AddGrpc();
var app = builder.Build();
app.Use(async (context, next) =>
{
    //логируем вошедший запрос
    //Console.WriteLine($"{DateTime.Now} Вошедший запрос -> {context.Request.Path}");
    await next.Invoke();
});
app.MapGrpcService<GrpcElkSearchService>();
app.MapControllers();
app.MapPrometheusScrapingEndpoint();
//запускаем веб-сервер и пишем в консоль хост и порт
ConsoleLogging.RunApp(app);
