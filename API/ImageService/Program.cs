
using ImageService.Consumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Common.Utils;
using ImageService.Data;
using AuctionService.Services;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

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
    p.AddConsumersFromNamespaceContaining<AuctionCreatingImageConsumer>();
    p.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("image", false));
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
builder.Services.AddGrpc(opt =>
{
    opt.EnableDetailedErrors = true;
    opt.MaxSendMessageSize = int.MaxValue;
    opt.MaxReceiveMessageSize = int.MaxValue;
});
builder.Services.AddOpenTelemetry()
    .WithMetrics(opt => opt
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("MetricGroup")))
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(builder.Configuration["Otlp:Endpoint"]);
        })
);
var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
// Configure the HTTP request pipeline.
app.UseAuthentication();
app.UseAuthorization();
app.MapGrpcService<GrpcImageServer>();
app.Run();