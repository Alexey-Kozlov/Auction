
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Polly;
using Common.Utils;
using FinanceService.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<FinanceDbContext>(options =>
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

// builder.Services.AddMassTransit(p =>
// {
//     p.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
//     p.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("image", false));
//     p.UsingRabbitMq((context, config) =>
//     {
//         config.UseMessageRetry(p =>
//         {
//             p.Handle<RabbitMqConnectionException>();
//             p.Interval(5, TimeSpan.FromSeconds(10));
//         });
//         config.Host(builder.Configuration["RabbitMq:Host"], "/", p =>
//         {
//             p.Username(builder.Configuration.GetValue("RabbitMq:UserName", "guest"));
//             p.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
//         });
//         config.ReceiveEndpoint("image-auction-created", e =>
//         {
//             e.UseMessageRetry(t => t.Interval(5, 5));
//             e.ConfigureConsumer<AuctionCreatedConsumer>(context);
//         });
//         config.ConfigureEndpoints(context);
//     });
// });

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
// Configure the HTTP request pipeline.
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
//Policy.Handle<NpgsqlException>().WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(10));

app.Run();