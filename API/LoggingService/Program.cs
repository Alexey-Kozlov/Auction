using Common.Contracts;
using Common.Utils.Vault;
using Confluent.Kafka;
using Logging.Consumers;
using Logging.Services;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddVault(options =>
          {
              var vaultOptions = builder.Configuration.GetSection("Vault");
              options.Address = vaultOptions["Address"];
              options.Role = vaultOptions["VAULT_ROLE_ID"];
              options.Secret = vaultOptions["VAULT_SECRET_ID"];
              options.SecretPathElk = vaultOptions["SecretPathElk"];
          });
builder.Services.AddControllers();
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = null;
});
builder.Services.AddMassTransit(busConfigurator =>
{
    busConfigurator.UsingInMemory((context, config) =>
    {
        config.ConfigureEndpoints(context, SnakeCaseEndpointNameFormatter.Instance);
    });
    busConfigurator.AddRider(r =>
                {
                    r.AddConsumer<LoggingConsumer>();
                    r.UsingKafka((context, k) =>
                    {
                        k.Host(builder.Configuration["Kafka_Host"]);
                        k.TopicEndpoint<ItemLoggingContract>(builder.Configuration["Kafka_Topic_Event"], "consumerGroup", e =>
                        {
                            e.ConfigureConsumer<LoggingConsumer>(context);
                            e.CreateIfMissing();
                        });
                    });
                });
});
builder.Services.AddScoped<ElkClient>();
var app = builder.Build();

app.Run();
