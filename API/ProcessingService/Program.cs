using System.Text;
using Common.Contracts;
using Common.Contracts.EventSourcing;
using Common.Utils;
using Common.Utils.Logging;
using Common.Utils.Settings;
using Common.Utils.Vault;
using Confluent.Kafka;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using ProcessingService.Data;
using ProcessingService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddVault(options =>
{
    var vaultOptions = builder.Configuration.GetSection("Vault");
    options.Address = vaultOptions["Address"];
    options.Role = vaultOptions["VAULT_ROLE_ID"];
    options.SecretPathPg = vaultOptions["SecretPathPg"];
    options.SecretPathRt = vaultOptions["SecretPathRt"];
    options.SecretPathApi = vaultOptions["SecretPathApi"];
    options.SecretPathKafka = vaultOptions["SecretPathKafka"];
    options.Secret = vaultOptions["VAULT_SECRET_ID"];
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = null;
});

builder.Services.AddControllers().AddJsonOptions(jsonOptions =>
{
    jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = null;
});
builder.Services.AddDbContextPool<ProcessingDbContext>(options =>
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

builder.Services.AddAuthentication(p =>
{
    p.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    p.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(p =>
{
    p.RequireHttpsMetadata = false;
    p.SaveToken = true;
    p.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["api:secret"])),
        ValidateIssuer = false,
        ValidateAudience = false,
        NameClaimType = "Login"
    };
});

//Шина для обработки сообщений RabbitMq
builder.Services.AddMassTransit(p =>
{
    p.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("processing", false));
    p.AddAuctionUpdateMassTransitConfigurator();
    p.AddAuctionDeleteMassTransitConfigurator();
    p.AddAuctionCreateMassTransitConfigurator();
    p.AddAuctionFinishMassTransitConfigurator();
    p.AddBidPlacedMassTransitConfigurator();
    p.ElkIndexMassTransitConfigurator();
    p.FinanceMassTransitConfigurator();
    p.EditNotificationMassTransitConfigurator();
    p.RestoreMassTransitConfigurator();
    p.SetSnapShotMassTransitConfigurator();
    p.AddCommunicationCreateMassTransitConfigurator();
    p.AddCommunicationDeleteMassTransitConfigurator();
    p.AddCommunicationUpdateMassTransitConfigurator();
    p.CurrentStateMassTransitConfigurator();
    p.DeleteTagMassTransitConfigurator();
    p.CreateTagMassTransitConfigurator();

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

//добавляем шину для обработки сообщений Kafka
builder.Services.AddMassTransit<ISecondBus>(busConfigurator =>
{
    busConfigurator.UsingInMemory((context, config) =>
    {
        config.ConfigureEndpoints(context, SnakeCaseEndpointNameFormatter.Instance);
    });
    busConfigurator.AddRider(r =>
    {
        r.AddProducer<ESContract>(builder.Configuration["kf:topic"], new ProducerConfig
        {
            MessageMaxBytes = 30485880,
            QueueBufferingMaxKbytes = 40000
        });
        r.UsingKafka((context, k) =>
        {
            k.Host(builder.Configuration["kf:host"]);
        });
    });
});


builder.Services.AddResourceMonitoring();
builder.Services.AddOpenTelemetry().WithMetrics(opt => opt
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("MetricGroup")))
    .AddProcessInstrumentation()
    .AddAspNetCoreInstrumentation()
    .AddMeter("Microsoft.Extensions.Diagnostics.ResourceMonitoring")
    .AddPrometheusExporter()
);

builder.Services.AddScoped<SendEventToES>();
builder.Services.AddScoped<SplitImages>();
//запускаем сервис по получению настроек системы - получаем параметр AdminMode - в административном ли
//режиме система. Если да - разрашаем работу с системой только администратору, остальным пользователям
//отдаем уведомление о работах в системе
builder.Services.AddSingleton<IsAdminModeService>();
builder.Services.AddHostedService(p => p.GetRequiredService<IsAdminModeService>());

builder.Services.AddHttpClient<SettingsHttpClient>(config =>
{
    config.Timeout = TimeSpan.FromSeconds(300);
});
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
//перехватываем исключение в http-запроса и возвращаем http-ответ с ошибкой - только для контроллеров
app.UseMiddleware<ExceptionMiddleware>();
app.MapControllers();
app.MapPrometheusScrapingEndpoint();
//запускаем веб-сервер и пишем в консоль хост и порт
ConsoleLogging.RunApp(app);
