using Common.Utils;
using Common.Utils.Logging;
using Common.Utils.Vault;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Services;

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
builder.Services.AddDbContext<SearchDbContext>(options =>
{
    var conStrBuilder = new NpgsqlConnectionStringBuilder();
    conStrBuilder.Password = builder.Configuration["pg:password"];
    conStrBuilder.Username = builder.Configuration["pg:username"];
    conStrBuilder.Database = builder.Configuration["pg:database"];
    conStrBuilder.Host = builder.Configuration["pg:host"];
    conStrBuilder.IncludeErrorDetail = true;

    options.UseNpgsql(conStrBuilder.ConnectionString);
});
builder.Services.AddGrpc();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//нигде не используется, оставлено для примера - это сервис синхронного вызова REST-сервиса
// builder.Services.AddHttpClient<AuctionSvcHttpClient>(config =>
// {
//     config.Timeout = TimeSpan.FromSeconds(300);
// });
builder.Services.AddScoped<SearchServiceSql>();
builder.Services.AddScoped<SearchProceduresService>();

builder.Services.AddMassTransit(p =>
{
    p.AddConsumersFromNamespaceContaining<AuctionConsumer>();
    p.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));
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

builder.Services.AddOpenTelemetry().WithMetrics(opt => opt
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("MetricGroup")))
    .AddProcessInstrumentation()
    .AddAspNetCoreInstrumentation()
    .AddMeter("Microsoft.AspNetCore.Hosting")
    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
    .AddPrometheusExporter()
);

var app = builder.Build();
//перехватываем исключение в http-запроса и возвращаем http-ответ с ошибкой - только для контроллеров
app.UseMiddleware<ExceptionMiddleware>();
//для корректной работе с датами в PostgreSql
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<GrpcFinanceService>();
app.MapGrpcService<GrpcReportService>();
//добавляет апи OTC к базовому эндпойнту, в деве это порт 7002
app.MapPrometheusScrapingEndpoint();
/*это не используется в функционале, для примера - подключение сервиса в конвейере,
выполняется 1 раз при старте приложения - 
app.Lifetime.ApplicationStarted.Register(async () =>
{
    await DbInitializer.InitDb(app);
});
*/

//запускаем веб-сервер и пишем в консоль хост и порт
ConsoleLogging.RunApp(app);
