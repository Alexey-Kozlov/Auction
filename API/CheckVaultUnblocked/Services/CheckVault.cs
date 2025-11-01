namespace CheckVaultUnblocked.Services;

public class CheckVault : IHostedService, IDisposable
{
    private readonly IConfiguration _configuration;
    private Timer _timer;
    public bool IsVaultRun { get; set; } = false;
    public readonly VaultHttpClient _vaultHttpClient;

    public CheckVault(IConfiguration configuration, VaultHttpClient vaultHttpClient)
    {
        _configuration = configuration;
        _vaultHttpClient = vaultHttpClient;
    }

    private async void DoCheck(object state)
    {
        // проверяем наличие значения конфигурации, связанного с волтом, 
        // если там пусто - сервис не работает
        IsVaultRun = await _vaultHttpClient.GetVaultStatus();
        // если волт разблокирован - прекращаем опросы
        if (IsVaultRun)
        {
            await StopAsync(new CancellationToken());
        }
    }

    public bool GetCheckVault() => IsVaultRun;

    public void Dispose()
    {
        if (_timer != null) _timer.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " - BackService is Starting... ");
        // периодический запуск метода, период - из настроек
        _timer = new Timer(DoCheck, null, TimeSpan.Zero,
            TimeSpan.FromSeconds(int.Parse(_configuration["RefreshPeriod"])));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " - BackService is Stopping... ");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }
}
