
using AuctionService.Data;
using ImageService.Consumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Polly;
using Common.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<ImageDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddMassTransit(p =>
{
    p.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    p.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("image", false));
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
        config.ReceiveEndpoint("image-auction-created", e =>
        {
            e.UseMessageRetry(t => t.Interval(5, 5));
            e.ConfigureConsumer<AuctionCreatedConsumer>(context);
        });
        config.ConfigureEndpoints(context);
    });
});

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
// Configure the HTTP request pipeline.

app.UseAuthorization();
app.MapControllers();
Policy.Handle<NpgsqlException>().WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(10));

app.Run();