using Microsoft.EntityFrameworkCore;
using TagService.Data;

namespace TagService.Services;

public class RefreshTags : IHostedService, IDisposable
{
    private readonly IServiceProvider _services;
    private Timer _timer;
    private readonly IConfiguration _configuration;

    public RefreshTags(IServiceProvider services, IConfiguration configuration)
    {
        _services = services;
        _configuration = configuration;
    }

    private async void DoRefresh(object state)
    {
        //периодически обновляем спиок с уникальными наименованиями тегов
        try
        {
            using (var scope = _services.CreateAsyncScope())
            {
                var _dbContext = scope.ServiceProvider.GetRequiredService<TagDbContext>();
                await _dbContext.Database.ExecuteSqlRawAsync("call tag_refresh_proc()");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("refresh Service Error {0}", ex.Message);
            throw;
        }
    }

    public void Dispose()
    {
        if (_timer != null) _timer.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " - Rfresh service is Starting... ");
        //как вариант - периодический запуск метода, например каждую минуту
        _timer = new Timer(DoRefresh, null, TimeSpan.Zero,
            TimeSpan.FromSeconds(int.Parse(_configuration["TagRefreshPeriod"])));
        return Task.CompletedTask;
    }
    //заглушка
    private void Fake(object state) { }
    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " - Stopping... ");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

}
