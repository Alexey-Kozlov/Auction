using System.Text;
using Common.Contracts;
using Common.Contracts.Logging;
using Common.Utils.Logging;
using Common.Utils.Vault;
using Confluent.Kafka;
using GatewayService.Consumers;
using GatewayService.Logging;
using GatewayService.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddVault(options =>
{
    var vaultOptions = builder.Configuration.GetSection("Vault");
    options.Address = vaultOptions["Address"];
    options.Role = vaultOptions["VAULT_ROLE_ID"];
    options.SecretPathRt = vaultOptions["SecretPathRt"];
    options.SecretPathApi = vaultOptions["SecretPathApi"];
    options.SecretPathKafka = vaultOptions["SecretPathKafka"];
    options.SecretPathRedis = vaultOptions["SecretPathRedis"];
    options.Secret = vaultOptions["VAULT_SECRET_ID"];
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = null;
});
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

//используем аутентификацию для YARP-proxy - в конфигураторе есть проверка некоторых запросов
//на аутентифицированность (параметр "AuthorizationPolicy": "default"), если отключить авторизацию - ошибка
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
        ValidateAudience = false
    };
});

builder.Services.AddMassTransit(p =>
{
    p.AddConsumersFromNamespaceContaining<GatewayConsumer>();
    p.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("gateway", false));
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

builder.Services.AddMassTransit<ISecondBus>(busConfigurator =>
{
    busConfigurator.UsingInMemory((context, config) =>
    {
        config.ConfigureEndpoints(context, SnakeCaseEndpointNameFormatter.Instance);
    });
    busConfigurator.AddRider(r =>
    {
        r.AddProducer<ItemLoggingContract>(builder.Configuration["kf:topiclog"], new ProducerConfig
        {
            MessageMaxBytes = 1000000
        });
        r.UsingKafka((context, k) =>
        {
            k.Host(builder.Configuration["kf:host"]);
        });
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("customPolicy", p =>
    {
        p.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithOrigins(builder.Configuration["ClientApp"]);
    });
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["rd:config"];
    options.InstanceName = builder.Configuration[""];
});
builder.Services.AddTransient<SendMessage>();
builder.Services.AddScoped<GrpcImageClient>();
builder.Services.AddSingleton<IDistributedCache, RedisCache>();
builder.Services.AddScoped<ImageCache>();
builder.Services.AddSingleton(cfg =>
{
    //конфигурация чтобы можно было сбрасывать кеш
    IConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(builder.Configuration["rd:config"]);
    return multiplexer;
});
builder.Services.AddSingleton<UserCurrentPage>();
builder.Services.AddGrpc();

builder.Services.AddResourceMonitoring();
builder.Services.AddOpenTelemetry().WithMetrics(opt => opt
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("MetricGroup")))
    .AddProcessInstrumentation()
    .AddAspNetCoreInstrumentation()
    .AddMeter("Microsoft.Extensions.Diagnostics.ResourceMonitoring")
    .AddPrometheusExporter()
);

var app = builder.Build();
app.UseCors("customPolicy");
//тут для отладки можно включить логирование поступающих запросов и выходящих ответов
// app.Use(async (context, next) =>
// {
//     // логируем вошедший запрос
//     Console.WriteLine(" Вошедший запрос -> " + context.Request.Path);
//     await next.Invoke();
//     // логируем ответ
// });


//первоначально - вызываем штатный функционал реверс-прокси YARP по переходу на нужные маршруты сервисов
//с помощью правил YARP маршрутизируем микросервисы
app.MapReverseProxy();
app.UseAuthentication();
app.UseAuthorization();
app.UseLoggingMiddleware();
//если ни один из маршрутов не сработал (в случае маршрута "/api/images" или "/api/images_file/{auctionid}") -
//переходим в расширение по маршрутизации на сервисы по обработке изображений
app.ImageMiddleware();
app.MapGrpcService<GrpcNotifyUsersService>();
app.MapPrometheusScrapingEndpoint();
//запускаем веб-сервер и пишем в консоль хост и порт
ConsoleLogging.RunApp(app);
