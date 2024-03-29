using BiddingService.Consumers;
using BiddingService.Data;
using BiddingService.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Polly;
using System.Text;
using Common.Utils;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<BidDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddControllers();

builder.Services.AddMassTransit(p =>
{
    p.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    p.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("bids", false));
    p.UsingRabbitMq((context, config) =>
    {
        config.UseMessageRetry(p =>
        {
            p.Handle<RabbitMqConnectionException>();
            p.Interval(5, TimeSpan.FromSeconds(10));
        });
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

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHostedService<CheckAuctionFinished>();
builder.Services.AddScoped<GrpcAuctionClient>();

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
// Configure the HTTP request pipeline.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Policy.Handle<NpgsqlException>().WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(10));

app.Run();
