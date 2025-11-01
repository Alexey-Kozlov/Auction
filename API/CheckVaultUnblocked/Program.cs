using CheckVaultUnblocked.Services;
using Common.Utils.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<CheckVault>();
builder.Services.AddHostedService(p => p.GetRequiredService<CheckVault>());

builder.Services.AddHttpClient<VaultHttpClient>(config =>
{
    config.Timeout = TimeSpan.FromSeconds(300);
});

var app = builder.Build();
app.MapControllers();
//запускаем веб-сервер и пишем в консоль хост и порт
ConsoleLogging.RunApp(app);

