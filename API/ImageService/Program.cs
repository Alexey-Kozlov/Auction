
using ImageService.Consumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Common.Utils;
using ImageService.Data;

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
        config.Host(builder.Configuration["RabbitMq:Host"], "/", p =>
        {
            p.Username(builder.Configuration.GetValue("RabbitMq:UserName", "guest"));
            p.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });
        config.ReceiveEndpoint("image-auction-created", e =>
        {
            e.ConfigureConsumer<AuctionCreatedConsumer>(context);
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

app.Run();