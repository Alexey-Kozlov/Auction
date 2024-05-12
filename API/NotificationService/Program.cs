using MassTransit;
using NotificationService.Consumers;
using NotificationService.Hubs;
using Common.Utils;
using NotificationService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
        });
        config.ReceiveEndpoint("finance-bid-finance-granting_error", e =>
        {
            e.ConfigureConsumer<BidFinanceGrantedFaultedConsumer>(context);
        });
        config.ReceiveEndpoint("bids-bid-placing_error", e =>
        {
            e.ConfigureConsumer<BidPlacedFaultedConsumer>(context);
        });
        config.ConfigureEndpoints(context);
    });
});
builder.Services.AddSignalR();

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/notifications");

app.Run();
