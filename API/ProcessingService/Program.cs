
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Common.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ProcessingService.Data;
using ProcessingService.Services;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Npgsql;
using ProcessingService;
using Common.Contracts;
using Confluent.Kafka;
using Common.Utils.Vault;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddVault(options =>
          {
              var vaultOptions = builder.Configuration.GetSection("Vault");
              options.Address = vaultOptions["Address"];
              options.Role = vaultOptions["VAULT_ROLE_ID"];
              options.SecretPathPg = vaultOptions["SecretPathPg"];
              options.SecretPathRt = vaultOptions["SecretPathRt"];
              options.SecretPathApi = vaultOptions["SecretPathApi"];
              options.Secret = vaultOptions["VAULT_SECRET_ID"];
          });
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = null;
});

builder.Services.AddControllers();
builder.Services.AddDbContext<ProcessingDbContext>(options =>
{
    var conStrBuilder = new NpgsqlConnectionStringBuilder();
    conStrBuilder.Password = builder.Configuration["pg:password"];
    conStrBuilder.Username = builder.Configuration["pg:username"];
    conStrBuilder.Database = builder.Configuration["pg:database"];
    conStrBuilder.Host = builder.Configuration["pg:host"];

    options.UseNpgsql(conStrBuilder.ConnectionString);
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["api:secret"])),
        ValidateIssuer = false,
        ValidateAudience = false,
        NameClaimType = "Login"
    };
});

//Шина для обработки сообщений RabbitMq
builder.Services.AddMassTransit(p =>
{
    p.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("processing", false));
    p.AddAuctionUpdateMassTransitConfigurator();
    p.AddAuctionDeleteMassTransitConfigurator();
    p.AddAuctionCreateMassTransitConfigurator();
    p.AddAuctionFinishMassTransitConfigurator();
    p.AddBidPlacedMassTransitConfigurator();
    p.ElkSearchMassTransitConfigurator();
    p.ElkIndexMassTransitConfigurator();

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

//добавляем шину для обработки сообщений Kafka
builder.Services.AddMassTransit<ISecondBus>(busConfigurator =>
{
    busConfigurator.UsingInMemory((context, config) =>
    {
        config.ConfigureEndpoints(context, SnakeCaseEndpointNameFormatter.Instance);
    });
    busConfigurator.AddRider(r =>
    {
        r.AddProducer<BaseStateContract>(builder.Configuration["Kafka_Topic_Event"], new ProducerConfig
        {
            MessageMaxBytes = 30485880,
            QueueBufferingMaxKbytes = 40000
        });
        r.UsingKafka((context, k) =>
        {
            k.Host(builder.Configuration["Kafka_Host"]);
        });
    });
});


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

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();