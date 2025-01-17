
using System.Reflection;
using Common.Contracts.Processing;
using EventSourcingService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Services;

public class CheckAuctionFinished : BackgroundService
{
    private readonly ILogger<CheckAuctionFinished> _logger;
    private readonly IServiceProvider _services;

    public CheckAuctionFinished(ILogger<CheckAuctionFinished> logger, IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Старт сервиса по проверке завершения аукционов...");
        stoppingToken.Register(() => _logger.LogInformation("Остановлен сервис по проверке завершения апукционов/"));
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckAuction(stoppingToken);
            await Task.Delay(5000, stoppingToken);
        }
    }

//using var transaction = _dbContext.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

    private async Task CheckAuction(CancellationToken stoppingToken)
    {
        using (var scope = _services.CreateAsyncScope())
        {
            var _publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            var _dbContext = scope.ServiceProvider.GetRequiredService<EventSourcingDbContext>();            
            var _configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var correlationId = Guid.NewGuid();
            using (var transaction = _dbContext.Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var finishedAuctions = _dbContext.check_auction_finished(correlationId);
                //возвращаем список записей для изменения соответствующих БД в нужных сервисах
                var listItems = new DataForProcessingServicesList
                {
                    DataObjects = new List<DataForProcessingService>()
                };
                foreach (var item in finishedAuctions.ToList())
                {
                    if ((CRUD)item.crud == CRUD.Read) continue;
                    listItems.DataObjects.Add
                    (
                        new DataForProcessingService
                        {
                            DataType = item.entitytype,
                            Data = item.eventdata,
                            CRUD = (CRUD)item.crud
                        }
                    );
                }
                if (listItems.DataObjects.Count() > 0)
                {
                    var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                        _configuration["CommonAssembly"]).CreateInstance("Common.Contracts.Processing.ESLog_AuctionFinish");
                    sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
                    sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, listItems);
                    await _publishEndpoint.Publish(sendObject);
                }
            }            
        }
    }

}
