using MassTransit;
using NotificationService.Consumers;
using NotificationService.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(p => 
{
    p.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    p.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("nt", false));
    p.UsingRabbitMq((context, config) => 
    {
        config.Host(builder.Configuration["RabbitMq:Host"], "/", p =>
        {
            p.Username(builder.Configuration.GetValue("RabbitMq:UserName","guest"));
            p.Password(builder.Configuration.GetValue("RabbitMq:Password","guest"));
        });
        config.ConfigureEndpoints(context);
    });
});
builder.Services.AddSignalR();

var app = builder.Build();

app.MapHub<NotificationHub>("/notifications");

app.Run();
