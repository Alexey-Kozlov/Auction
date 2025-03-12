using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GatewayService.Services;
using GatewayService.Cache;
using MassTransit;
using GatewayService.Consumers;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Common.Utils.Vault;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;
using GatewayService.Logging;
using Common.Contracts;
using Confluent.Kafka;
using Common.Utils;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddVault(options =>
          {
              var vaultOptions = builder.Configuration.GetSection("Vault");
              options.Address = vaultOptions["Address"];
              options.Role = vaultOptions["VAULT_ROLE_ID"];
              options.SecretPathRt = vaultOptions["SecretPathRt"];
              options.SecretPathApi = vaultOptions["SecretPathApi"];
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
        r.AddProducer<ItemLoggingContract>(builder.Configuration["Kafka_Topic_Event"], new ProducerConfig
        {
            MessageMaxBytes = 1000000
        });
        r.UsingKafka((context, k) =>
        {
            k.Host(builder.Configuration["Kafka_Host"]);
        });
    });
});
builder.Services.AddTransient<SendMessage>();
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
    options.Configuration = builder.Configuration["Redis:Config"];
    options.InstanceName = "AuctionCache";
});

builder.Services.AddScoped<GrpcImageClient>();
builder.Services.AddSingleton<IDistributedCache, RedisCache>();
builder.Services.AddScoped<ImageCache>();
builder.Services.AddSingleton(cfg =>
{
    //конфигурация чтобы можно было сбрасывать кеш
    IConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(builder.Configuration["Redis:Config"]);
    return multiplexer;
});



builder.Services.AddOpenTelemetry()
    .WithMetrics(opt => opt
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("MetricGroup")))
        .AddProcessInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(builder.Configuration["Otlp:Endpoint"]);
        })
);

var app = builder.Build();
app.UseCors("customPolicy");
app.UseMiddleware<ExceptionMiddleware>();
// app.Use(async (context, next) =>
// {
//     // логируем вошедший запрос
//     Console.WriteLine(" Вошедший запрос -> " + context.Request.Path);
//     await next.Invoke();
//     // логируем ответ
// });

// добавляем дополнительный роутинг для возврата изображений
// это все запросы начинающиеся с :
//    /api/images/*
//    /api/images_dop/*
//    /api/images_file/*
// если это такой запрос - дальше запрос не проходит, возвращается изображение или null


//если запрашивается не изображение - проходим сюда и вызываем штатный функционал реверс-прокси YARP
//с помощью правил YARP маршрутизируем микросервисы
app.MapReverseProxy();
app.UseAuthentication();
app.UseAuthorization();
app.UseLoggingMiddleware();
app.ImageMiddleware();
app.Run();
