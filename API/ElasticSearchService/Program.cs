using Common.Utils;
using Common.Utils.Vault;
using ElasticSearchService.Consumers;
using ElasticSearchService.Data;
using ElasticSearchService.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddVault(options =>
          {
              var vaultOptions = builder.Configuration.GetSection("Vault");
              options.Address = vaultOptions["Address"];
              options.Role = vaultOptions["VAULT_ROLE_ID"];
              options.SecretPathPg = vaultOptions["SecretPathPg"];
              options.SecretPathRt = vaultOptions["SecretPathRt"];
              options.SecretPathElk = vaultOptions["SecretPathElk"];
              options.Secret = vaultOptions["VAULT_SECRET_ID"];
          });
builder.Services.AddControllers();
builder.Services.AddDbContext<SearchDbContext>(options =>
{
    var conStrBuilder = new NpgsqlConnectionStringBuilder();
    conStrBuilder.Password = builder.Configuration["pg:password"];
    conStrBuilder.Username = builder.Configuration["pg:username"];
    conStrBuilder.Database = builder.Configuration["pg:database"];
    conStrBuilder.Host = builder.Configuration["pg:host"];

    options.UseNpgsql(conStrBuilder.ConnectionString);
});
builder.Services.AddMassTransit(p =>
{
    p.AddConsumersFromNamespaceContaining<AuctionCreatingElkConsumer>();
    p.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("elk", false));
    p.UsingRabbitMq((context, config) =>
    {
        config.Host(builder.Configuration["rt:host"], "/", p =>
        {
            p.Username(builder.Configuration["rt:password"]);
            p.Password(builder.Configuration["rt:password"]);
        });
        config.ConfigureEndpoints(context);
    });
});

builder.Services.AddScoped<ElkClient>();
builder.Services.AddOpenTelemetry()
    .WithMetrics(opt => opt
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("MetricGroup")))
        .AddProcessInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(builder.Configuration["Otlp:Endpoint"]);
        })
);

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
app.Use(async (context, next) =>
{
    //логируем вошедший запрос
    Console.WriteLine($"{DateTime.Now} Вошедший запрос -> {context.Request.Path}");
    await next.Invoke();
    // Do logging or other work that doesn't write to the Response.
});
app.MapControllers();
app.Run();
