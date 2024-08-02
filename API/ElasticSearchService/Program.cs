using ElasticSearchService.Consumers;
using ElasticSearchService.Data;
using ElasticSearchService.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddDbContext<SearchDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddMassTransit(p =>
{
    p.AddConsumersFromNamespaceContaining<AuctionCreatingElkConsumer>();
    p.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("elk", false));
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

builder.Services.AddScoped<ElkClient>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    //логируем вошедший запрос
    Console.WriteLine($"{DateTime.Now} Вошедший запрос -> {context.Request.Path}");
    await next.Invoke();
    // Do logging or other work that doesn't write to the Response.
});
app.MapControllers();
app.Run();