using MassTransit;
using NotificationService.Consumers;
using NotificationService.Hubs;
using Common.Utils;
using NotificationService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddDbContext<NotificationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
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
    p.AddConsumersFromNamespaceContaining<AuctionCreatingNotificationConsumer>();
    p.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("notification", false));
    p.UsingRabbitMq((context, config) =>
    {
        config.Host(builder.Configuration["RabbitMq:Host"], "/", p =>
        {
            p.Username(builder.Configuration.GetValue("RabbitMq:UserName", "guest"));
            p.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });
        config.ReceiveEndpoint("auction-bid-auction-placing_error", e =>
        {
            e.ConfigureConsumer<BidAuctionPlacedFaultedConsumer>(context);
            e.DiscardSkippedMessages();
        });
        config.ReceiveEndpoint("finance-bid-finance-granting_error", e =>
        {
            e.ConfigureConsumer<BidFinanceGrantedFaultedConsumer>(context);
            e.DiscardSkippedMessages();
        });
        config.ReceiveEndpoint("bids-bid-placing_error", e =>
        {
            e.ConfigureConsumer<BidPlacedFaultedConsumer>(context);
            e.DiscardSkippedMessages();
        });
        config.ReceiveEndpoint("search-bid-search-placing_error", e =>
        {
            e.ConfigureConsumer<BidSearchPlacedFaultedConsumer>(context);
            e.DiscardSkippedMessages();
        });
        config.ConfigureEndpoints(context);
    });
});
builder.Services.AddSignalR();
builder.Services.AddOpenTelemetry()
    .WithMetrics(opt => opt
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("MeterName")))
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(builder.Configuration["Otlp:Endpoint"]);
        })
);

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/notifications");

app.Run();
