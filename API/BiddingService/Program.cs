using System.Text;
using BiddingService.Consumers;
using BiddingService.Data;
using BiddingService.Services;
using Common.Utils;
using Common.Utils.Logging;
using Common.Utils.Settings;
using Common.Utils.Vault;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
              options.SecretPathApi = vaultOptions["SecretPathApi"];
              options.Secret = vaultOptions["VAULT_SECRET_ID"];
          });
builder.Services.AddDbContext<BidDbContext>(options =>
{
    var conStrBuilder = new NpgsqlConnectionStringBuilder();
    conStrBuilder.Password = builder.Configuration["pg:password"];
    conStrBuilder.Username = builder.Configuration["pg:username"];
    conStrBuilder.Database = builder.Configuration["pg:database"];
    conStrBuilder.Host = builder.Configuration["pg:host"];
    conStrBuilder.IncludeErrorDetail = true;

    options.UseNpgsql(conStrBuilder.ConnectionString);
});
builder.Services.AddControllers();

builder.Services.AddMassTransit(p =>
{
    p.AddConsumersFromNamespaceContaining<BidConsumer>();
    p.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("bids", false));
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

builder.Services.AddAutoMapper(p => { }, typeof(Program).Assembly);

builder.Services.AddResourceMonitoring();
builder.Services.AddOpenTelemetry().WithMetrics(opt => opt
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("MetricGroup")))
    .AddProcessInstrumentation()
    .AddAspNetCoreInstrumentation()
    .AddMeter("Microsoft.Extensions.Diagnostics.ResourceMonitoring")
    .AddPrometheusExporter()
);
builder.Services.AddScoped<BidProceduresService>();
builder.Services.AddScoped<GetBidsService>();
builder.Services.AddGrpc();
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
app.MapGrpcService<GrpcReportService>();
app.MapPrometheusScrapingEndpoint();
//запускаем веб-сервер и пишем в консоль хост и порт
ConsoleLogging.RunApp(app);
