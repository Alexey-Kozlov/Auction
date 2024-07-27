
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchService;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Services;
using Common.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<SearchDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddHttpClient<AuctionSvcHttpClient>(config =>
{
    config.Timeout = TimeSpan.FromSeconds(300);
});

builder.Services.AddMassTransit(p =>
{
    p.AddConsumersFromNamespaceContaining<AuctionCreatingSearchConsumer>();
    p.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));
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

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
// Configure the HTTP request pipeline.
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
//инициализация БД поиска из сервиса Auction
app.Lifetime.ApplicationStarted.Register(async () =>
{
    await DbInitializer.InitDb(app);
});

app.Run();
