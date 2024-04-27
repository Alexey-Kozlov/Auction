
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Common.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ProcessingService.Data;
using ProcessingService.StateMachines;
using Contracts;
using ProcessingService.Consumers;

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
    p.AddConsumersFromNamespaceContaining<FaultedDebitAddConsumer>();
    p.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("processing", false));
    p.AddSagaStateMachine<ProcessingStateMachine, ProcessingState>((context, cfg) =>
    {
        cfg.UseInMemoryOutbox(context);
    })
    //.InMemoryRepository()
    .EntityFrameworkRepository(p =>
    {
        p.ConcurrencyMode = ConcurrencyMode.Pessimistic;
        p.ExistingDbContext<ProcessingDbContext>();
        p.UsePostgres();
    });
    p.UsingRabbitMq((context, config) =>
    {
        config.Host(builder.Configuration["RabbitMq:Host"], "/", p =>
        {
            p.Username(builder.Configuration.GetValue("RabbitMq:UserName", "guest"));
            p.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });
        config.ReceiveEndpoint("finance-debit-add_error", e =>
        {
            e.ConfigureConsumer<FaultedDebitAddConsumer>(context);
        });
        config.ReceiveEndpoint("bids-bid-placed_error", e =>
        {
            e.ConfigureConsumer<FaultedBidPlaceConsumer>(context);
        });
        config.ConfigureEndpoints(context);

    });
});
EndpointConvention.Map<RequestFinanceDebitAdd>(new Uri("queue:finance-debit-add"));
EndpointConvention.Map<RequestBidPlace>(new Uri("queue:bids-bid-placed"));
EndpointConvention.Map<UserNotificationSet>(new Uri("queue:notification-set-notification"));
EndpointConvention.Map<RollbackFinanceDebitAdd>(new Uri("queue:finance-rollback-debit-add"));
EndpointConvention.Map<Fault<RequestFinanceDebitAdd>>(new Uri("queue:finance-debit-add_error"));
EndpointConvention.Map<Fault<RequestBidPlace>>(new Uri("queue:bids-bid-placed_error"));

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
// Configure the HTTP request pipeline.
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();