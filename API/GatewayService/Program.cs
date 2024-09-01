using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Common.Utils;
using GatewayService.Services;
using GatewayService.Cache;
using MassTransit;
using GatewayService.Consumers;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = null;
});
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration.GetValue<string>("ApiSettings:Secret"))),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddMassTransit(p =>
{
    p.AddConsumersFromNamespaceContaining<AuctionUpdatingGatewayConsumer>();
    p.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("gateway", false));
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
});

builder.Services.AddScoped<GrpcImageClient>();
builder.Services.AddScoped<ImageCache>();

builder.Services.AddOpenTelemetry()
    .WithMetrics(opt => opt
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("MetricGroup")))
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(builder.Configuration["Otlp:Endpoint"]);
        })
);

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("customPolicy");

app.Use(async (context, next) =>
{
    // логируем вошедший запрос
    Console.WriteLine(" Вошедший запрос -> " + context.Request.Path);
    await next.Invoke();
    // логируем ответ
});

Console.WriteLine($"{DateTime.Now} - Gateway service started");

// добавляем дополнительный роутинг для возврата изображений
// это все запросы начинающиеся с :
//    /api/images/*
//    /api/images_dop/*
//    /api/images_file/*
// если это такой запрос - дальше запрос не проходит, возвращается изображение или null
app.ImageMiddleware();

//если запрашивается не изображение - проходим сюда и вызываем штатный функционал реверс-прокси YARP
//с помощью правил YARP маршрутизируем микросервисы
app.MapReverseProxy();
app.UseAuthentication();
app.UseAuthorization();

app.Run();
