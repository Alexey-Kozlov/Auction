using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Common.Utils.Settings;

public class IsAdminModeService : IHostedService, IDisposable
{
    private readonly IConfiguration _configuration;
    private Timer _timer;
    private SettingsDTO result;
    public readonly SettingsHttpClient _settingsHttpClient;

    public IsAdminModeService(IConfiguration configuration, SettingsHttpClient settingsHttpClient)
    {
        _configuration = configuration;
        _settingsHttpClient = settingsHttpClient;
    }

    private async void DoCheck(object state)
    {
        // проверяем наличие значения конфигурации, если там пусто - сервис не работает, 
        //повторяем через некоторое время
        result = await _settingsHttpClient.GetAdminModeSetting();
        // если данные получены - прекращаем опросы
        if (result.CheckResult)
        {
            await StopAsync(new CancellationToken());
        }
    }

    public SettingsDTO GetAdminMode() => result;

    public void SetAdminMode(bool AdminMode)
    {
        result.AdminMode = AdminMode;
    }

    public void Dispose()
    {
        if (_timer != null) _timer.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " - Check settings is starting... ");
        // периодический запуск метода, период - из настроек
        _timer = new Timer(DoCheck, null, TimeSpan.Zero,
            TimeSpan.FromSeconds(int.Parse(_configuration["SettingsCheckPeriod"])));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " - Check settings is Stopping... ");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }
}
