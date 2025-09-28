using System.Text;
using Common.Contracts;
using Common.Contracts.EventSourcing;
using Common.Utils;
using Common.Utils.Logging;
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
builder.Services.AddDbContext<ProcessingDbContext>(options =>
{
    var conStrBuilder = new NpgsqlConnectionStringBuilder();
    conStrBuilder.Password = builder.Configuration["pg:password"];
    conStrBuilder.Username = builder.Configuration["pg:username"];
    conStrBuilder.Database = builder.Configuration["pg:database"];
    conStrBuilder.Host = builder.Configuration["pg:host"];
    conStrBuilder.IncludeErrorDetail = true;

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
    p.ElkSearchMassTransitConfigurator();
    p.ElkIndexMassTransitConfigurator();
    p.FinanceMassTransitConfigurator();
    p.EditNotificationMassTransitConfigurator();
    p.RestoreMassTransitConfigurator();
    p.SetSnapShotMassTransitConfigurator();
    p.AddCommunicationCreateMassTransitConfigurator();
    p.AddCommunicationDeleteMassTransitConfigurator();
    p.AddCommunicationUpdateMassTransitConfigurator();

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


builder.Services.AddOpenTelemetry().WithMetrics(opt => opt
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("MetricGroup")))
    .AddProcessInstrumentation()
    .AddAspNetCoreInstrumentation()
    .AddMeter("Microsoft.AspNetCore.Hosting")
    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
    .AddPrometheusExporter()
);

builder.Services.AddScoped<SendEventToES>();
builder.Services.AddScoped<SplitImages>();

var app = builder.Build();

//перехватываем исключение в http-запроса и возвращаем http-ответ с ошибкой - только для контроллеров
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapPrometheusScrapingEndpoint();
//запускаем веб-сервер и пишем в консоль хост и порт
ConsoleLogging.RunApp(app);
