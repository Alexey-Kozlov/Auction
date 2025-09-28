using System.Text;
using Common.Utils;
using Common.Utils.Logging;
using Common.Utils.Vault;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using ReportService.Reports;
using ReportService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddVault(options =>
          {
              var vaultOptions = builder.Configuration.GetSection("Vault");
              options.Address = vaultOptions["Address"];
              options.Role = vaultOptions["VAULT_ROLE_ID"];
              options.SecretPathApi = vaultOptions["SecretPathApi"];
              options.Secret = vaultOptions["VAULT_SECRET_ID"];
          });
//конфигурация конвейера для работы с JWT-аутентификацией
//нужно для допуска к контроллеру отчетов только аутентифицированных пользователей
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
builder.Services.AddControllers().AddJsonOptions(jsonOptions =>
{
    jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = null;
});
builder.Services.AddOpenTelemetry().WithMetrics(opt => opt
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("MetricGroup")))
    .AddProcessInstrumentation()
    .AddAspNetCoreInstrumentation()
    .AddMeter("Microsoft.AspNetCore.Hosting")
    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
    .AddPrometheusExporter()
);
builder.Services.AddCors();
builder.Services.AddScoped<AuctionList>();
builder.Services.AddScoped<AuctionListTree>();
builder.Services.AddScoped<NotificationList>();
builder.Services.AddScoped<Diagrams>();
builder.Services.AddScoped<GrpcReportsClient>();

var app = builder.Build();
//перехватываем исключение в http-запроса и возвращаем http-ответ с ошибкой - только для контроллеров
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors(p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().WithExposedHeaders("*"));
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapPrometheusScrapingEndpoint();
//запускаем веб-сервер и пишем в консоль хост и порт
ConsoleLogging.RunApp(app);

