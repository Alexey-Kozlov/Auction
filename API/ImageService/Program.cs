
using Common.Utils.Logging;
using Common.Utils.Vault;
using ImageService.Consumers;
using ImageService.Data;
using ImageService.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddVault(options =>
{
    var vaultOptions = builder.Configuration.GetSection("Vault");
    options.Address = vaultOptions["Address"];
    options.Role = vaultOptions["VAULT_ROLE_ID"];
    options.SecretPathPg = vaultOptions["SecretPathPg"];
    options.SecretPathRt = vaultOptions["SecretPathRt"];
    options.Secret = vaultOptions["VAULT_SECRET_ID"];
});
builder.Services.AddControllers();
builder.Services.AddDbContextPool<ImageDbContext>(options =>
{
    var conStrBuilder = new NpgsqlConnectionStringBuilder
    {
        Password = builder.Configuration["pg:password"],
        Username = builder.Configuration["pg:username"],
        Database = builder.Configuration["pg:database"],
        Host = builder.Configuration["pg:host"],
        IncludeErrorDetail = true,
        MaxPoolSize = int.Parse(builder.Configuration["pg:maxpoolsize"]),
        CommandTimeout = int.Parse(builder.Configuration["pg:commandtimeout"])
    };

    options.UseNpgsql(conStrBuilder.ConnectionString);
});
builder.Services.AddAutoMapper(p => { }, typeof(Program).Assembly);
builder.Services.AddMassTransit(p =>
{
    p.AddConsumersFromNamespaceContaining<ImageConsumer>();
    p.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("image", false));
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
builder.Services.AddGrpc(opt =>
{
    opt.EnableDetailedErrors = true;
    opt.MaxSendMessageSize = int.MaxValue;
    opt.MaxReceiveMessageSize = int.MaxValue;
});

builder.Services.AddResourceMonitoring();
builder.Services.AddOpenTelemetry().WithMetrics(opt => opt
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("MetricGroup")))
    .AddProcessInstrumentation()
    .AddAspNetCoreInstrumentation()
    .AddMeter("Microsoft.Extensions.Diagnostics.ResourceMonitoring")
    .AddPrometheusExporter()
);

builder.Services.AddSingleton<RestoreImageService>();
builder.Services.AddScoped<ImageProceduresService>();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapGrpcService<GrpcImageServer>();
app.MapPrometheusScrapingEndpoint();
//запускаем веб-сервер и пишем в консоль хост и порт
ConsoleLogging.RunApp(app);