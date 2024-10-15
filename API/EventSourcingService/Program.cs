using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Common.Utils;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using AuctionService.Metrics;
using Npgsql;
using Common.Contracts;
using EventSourcingService.Data;
using EventSourcingService;
using SearchService.Consumers;
using EventSourcingService.Consumers;

internal class Program
{
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

            options.UseNpgsql(conStrBuilder.ConnectionString);

        });
        builder.Services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.AddConsumersFromNamespaceContaining<AuctionItemsListConsumer>();
            busConfigurator.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));
            busConfigurator.UsingRabbitMq((context, config) =>
            {
                config.Host(builder.Configuration["rt:host"], "/", p =>
                {
                    p.Username(builder.Configuration["rt:password"]);
                    p.Password(builder.Configuration["rt:password"]);
                });
                config.ConfigureEndpoints(context);
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
                r.AddConsumer<EventSourcingEventConsumer>();
                r.UsingKafka((context, k) =>
                {
                    k.Host(builder.Configuration["Kafka_Host"]);
                    k.TopicEndpoint<BaseStateContract>(builder.Configuration["Kafka_Topic_Event"], "consumerGroup", e =>
                    {
                        e.ConfigureConsumer<EventSourcingEventConsumer>(context);
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

        builder.Services.AddOpenTelemetry()
            .WithMetrics(opt => opt
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("MetricConfig:MetricGroup")))
                //.AddAspNetCoreInstrumentation()
                //.AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(builder.Configuration["Otlp:Endpoint"]);
                })
            )
            .WithMetrics(opt => opt
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("MetricConfig:MetricCustom:MetricGroup")))
                .AddMeter(builder.Configuration.GetValue<string>("MetricConfig:MetricCustom:MetricGroup"))
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(builder.Configuration["Otlp:Endpoint"]);
                })
            );

        builder.Services.AddSingleton<AuctionMetrics>();

        var app = builder.Build();


        app.Use(async (context, next) =>
        {
            //логируем вошедший запрос
            Console.WriteLine($"{DateTime.Now} Вошедший запрос -> {context.Request.Path}");
            await next.Invoke();
        });
        app.UseMiddleware<ExceptionMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}