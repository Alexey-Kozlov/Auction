using Common.Contracts;
using Common.Contracts.Logging;
using Common.Utils.Logging;
using Common.Utils.Vault;
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
              options.SecretPathRt = vaultOptions["SecretPathRt"];
              options.SecretPathElk = vaultOptions["SecretPathElk"];
          });
builder.Services.AddControllers();
builder.Services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.AddConsumersFromNamespaceContaining<LoggingServiceErrorConsumer>();
            busConfigurator.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("logging", false));
            busConfigurator.UsingRabbitMq((context, config) =>
            {
                config.Host(builder.Configuration["rt:host"], "/", p =>
                {
                    p.Username(builder.Configuration["rt:username"]);
                    p.Password(builder.Configuration["rt:password"]);
                });
                config.ConfigureEndpoints(context);
                config.ConcurrentMessageLimit = 1;
            });
        });
builder.Services.AddMassTransit<ISecondBus>(busConfigurator =>
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

//запускаем веб-сервер и пишем в консоль хост и порт
ConsoleLogging.RunApp(app);
