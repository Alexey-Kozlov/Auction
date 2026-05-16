using System.Text;
using Common.Utils;
using Common.Utils.Logging;
using Common.Utils.Settings;
using Common.Utils.Vault;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NotificationService.Consumers;
using NotificationService.Data;
using NotificationService.Hubs;
using NotificationService.Services;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using ReportService.Services;

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
builder.Services.AddControllers();
builder.Services.AddDbContextPool<NotificationDbContext>(options =>
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
                        ValidateAudience = false
                    };
                    p.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notifications"))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
builder.Services.AddMassTransit(p =>
{
    p.AddConsumersFromNamespaceContaining<BidConsumer>();
    p.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("notification", false));
    p.UsingRabbitMq((context, config) =>
    {
        config.Host(builder.Configuration["rt:host"], "/", p =>
        {
            p.Username(builder.Configuration["rt:username"]);
            p.Password(builder.Configuration["rt:password"]);
        });
        // config.ReceiveEndpoint("search-bid-search-placing_error", e =>
        // {
        //     e.ConfigureConsumer<BidSearchPlacedFaultedConsumer>(context);
        //     e.DiscardSkippedMessages();
        // });
        config.ConfigureEndpoints(context);
        config.ConcurrentMessageLimit = 1;
    });
});
builder.Services.AddSignalR();

builder.Services.AddResourceMonitoring();
builder.Services.AddOpenTelemetry().WithMetrics(opt => opt
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("MetricGroup")))
    .AddProcessInstrumentation()
    .AddAspNetCoreInstrumentation()
    .AddMeter("Microsoft.Extensions.Diagnostics.ResourceMonitoring")
    .AddPrometheusExporter()
);
builder.Services.AddScoped<NotifyProceduresService>();
builder.Services.AddGrpc();
builder.Services.AddScoped<GrpcUsersNotifyClient>();
builder.Services.AddScoped<GetNotifyService>();
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
app.MapHub<NotificationHub>("/notifications");
app.MapPrometheusScrapingEndpoint();
//запускаем веб-сервер и пишем в консоль хост и порт
ConsoleLogging.RunApp(app);
