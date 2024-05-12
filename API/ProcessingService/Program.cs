
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Common.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ProcessingService.Data;
using ProcessingService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<ProcessingDbContext>(options =>
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
        ValidateAudience = false,
        NameClaimType = "Login"
    };
});

builder.Services.AddMassTransit(p =>
{
    p.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("processing", false));
    p.AddAuctionUpdateMassTransitConfigurator();
    p.AddAuctionDeleteMassTransitConfigurator();
    p.AddAuctionCreateMassTransitConfigurator();
    p.AddAuctionFinishMassTransitConfigurator();
    p.AddBidPlacedMassTransitConfigurator();

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

builder.Services.AddAuctionUpdateServices(builder);
builder.Services.AddAuctionDeleteServices(builder);
builder.Services.AddAuctionCreateServices(builder);
builder.Services.AddAuctionFinishServices(builder);
builder.Services.AddBidPlacedServices(builder);

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
// Configure the HTTP request pipeline.
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();