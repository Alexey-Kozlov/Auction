using Common.Utils.Vault;
using ReportService.Reports;
using ReportService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient<HttpClientService>(config =>
{
    config.Timeout = TimeSpan.FromSeconds(300);
});
builder.Configuration.AddVault(options =>
          {
              var vaultOptions = builder.Configuration.GetSection("Vault");
              options.Address = vaultOptions["Address"];
              options.Role = vaultOptions["VAULT_ROLE_ID"];
              options.SecretPathApi = vaultOptions["SecretPathApi"];
              options.Secret = vaultOptions["VAULT_SECRET_ID"];
          });
builder.Services.AddControllers();
builder.Services.AddCors();
builder.Services.AddScoped<GetDataService>();
builder.Services.AddScoped<AuctionList>();
builder.Services.AddScoped<NotificationList>();
var app = builder.Build();
app.UseCors(p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().WithExposedHeaders("*"));
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

