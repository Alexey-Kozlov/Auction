using System.Text;
using AuctionService.Consumers;
using AuctionService.Data;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Common.Utils;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using AuctionService.Metrics;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddDbContext<AuctionDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
        });
        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        builder.Services.AddMassTransit(p =>
        {
            p.AddConsumersFromNamespaceContaining<AuctionCreatingConsumer>();
            p.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));
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
            // Do logging or other work that doesn't write to the Response.
        });
        app.UseMiddleware<ExceptionMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        //DbInit.InitDb(app);

        app.Run();
    }
}