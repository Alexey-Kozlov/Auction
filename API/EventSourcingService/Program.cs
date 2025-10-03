using System.Text;
using AuctionService.Metrics;
using Common.Contracts;
using Common.Contracts.EventSourcing;
using Common.Utils.Logging;
using Common.Utils.Vault;
using EventSourcingService.Consumers;
using EventSourcingService.Data;
using EventSourcingService.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using SearchService.Consumers;

internal class Program
{
    //старая нотация запуска веб-сервера
    private static void Main(string[] args)
    {
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
        builder.Services.AddControllers();

        builder.Services.AddDbContext<EventSourcingDbContext>(options =>
        {
            var conStrBuilder = new NpgsqlConnectionStringBuilder();
            conStrBuilder.Password = builder.Configuration["pg:password"];
            conStrBuilder.Username = builder.Configuration["pg:username"];
            conStrBuilder.Database = builder.Configuration["pg:database"];
            conStrBuilder.Host = builder.Configuration["pg:host"];
            conStrBuilder.Timeout = 300;
            conStrBuilder.CommandTimeout = 300;
            conStrBuilder.IncludeErrorDetail = true;

            options.UseNpgsql(conStrBuilder.ConnectionString);
        }, ServiceLifetime.Transient, ServiceLifetime.Transient);

        builder.Services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.AddConsumersFromNamespaceContaining<SetSnapShotConsumer>();
            busConfigurator.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("eventsourcing", false));
            busConfigurator.UsingRabbitMq((context, config) =>
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
                r.AddConsumer<CreateEventSourcingItemConsumer>();
                r.UsingKafka((context, k) =>
                {
                    k.Host(builder.Configuration["kf:host"]);
                    k.TopicEndpoint<ESContract>(builder.Configuration["kf:topic"], "consumerGroup", e =>
                    {
                        e.ConfigureConsumer<CreateEventSourcingItemConsumer>(context);
                        e.CreateIfMissing();
                    });
                });
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
        builder.Services.AddResourceMonitoring();
        builder.Services.AddOpenTelemetry()
            .WithMetrics(opt => opt
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("MetricConfig:MetricGroup")))
                .AddProcessInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddMeter("Microsoft.Extensions.Diagnostics.ResourceMonitoring")
                .AddPrometheusExporter()
            )
            .WithMetrics(opt => opt
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("MetricConfig:MetricCustom:MetricGroup")))
                .AddMeter(builder.Configuration.GetValue<string>("MetricConfig:MetricCustom:MetricGroup"))
                .AddPrometheusExporter()
            );
        builder.Services.AddSingleton<CheckAuctionFinished>();
        builder.Services.AddHostedService(p => p.GetRequiredService<CheckAuctionFinished>());
        builder.Services.AddSingleton<AuctionMetrics>();
        builder.Services.AddScoped<ElkIndexProcessing>();
        builder.Services.AddScoped<AuctionDeleteProcessing>();
        builder.Services.AddScoped<AuctionCreateProcessing>();
        builder.Services.AddScoped<AuctionUpdateProcessing>();
        builder.Services.AddScoped<ESLogCommitProcessing>();
        builder.Services.AddScoped<FinanceCreateProcessing>();
        builder.Services.AddScoped<BidPlaceProcessing>();
        builder.Services.AddScoped<EditNotificationProcessing>();
        builder.Services.AddScoped<RestoreSnapShotProcessing>();
        builder.Services.AddScoped<CommunicationCreateProcessing>();
        builder.Services.AddScoped<CommunicationDeleteProcessing>();
        builder.Services.AddScoped<CommunicationUpdateProcessing>();
        builder.Services.AddSingleton<RestoreImageService>();

        var app = builder.Build();
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapPrometheusScrapingEndpoint();
        //запускаем веб-сервер и пишем в консоль хост и порт
        ConsoleLogging.RunApp(app);
    }
}