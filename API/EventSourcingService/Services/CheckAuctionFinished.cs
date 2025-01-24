using System.Reflection;
using AuctionService.Metrics;
using Common.Contracts.Processing;
using Common.Contracts.EventSourcing;
using EventSourcingService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Collections.Concurrent;

namespace EventSourcingService.Services;

public class CheckAuctionFinished : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly AuctionMetrics _auctionMetrics;
    private ConcurrentDictionary<Guid, CancellationTokenSource> tokens = new();

    public CheckAuctionFinished(IServiceProvider services, AuctionMetrics auctionMetrics)
    {
        _services = services;
        _auctionMetrics = auctionMetrics;
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //получаем даты, когда должны завершиться аукционы
        //заполняем список задач на завершение и запускаем задания на завершение 
        //при наступлении даты завершения аукциона
        foreach (var auction in GetAuctionsToFinish())
        {
            var cancelTokenSource = new CancellationTokenSource();
            tokens[auction.AuctionId] = cancelTokenSource;
            Task.Run(async () =>
            {
                await DelayFinishAuction(tokens[auction.AuctionId].Token, auction);
            }, tokens[auction.AuctionId].Token);
        }
        return Task.CompletedTask;
    }

    private async Task DelayFinishAuction(CancellationToken ct, AuctionFinishedData auctionData)
    {
        long delay = (long)(auctionData.AuctionEnd - DateTime.UtcNow).TotalMilliseconds;
        //на долгих периодах длительности - количество миллисекунд превышает максимальное значение
        //Integer, поэтому делаем цикл из нескольких задержек
        while (delay > 0)
        {
            var currentDelay = delay > int.MaxValue ? int.MaxValue : (int)delay;
            await Task.Delay(currentDelay);
            delay -= currentDelay;
        }
        //если было создание, обновление или удаление аукциона - отменяем ВСЕ ранее запущенные задания
        //ВАЖНО - отмена произойдет в момент истечения срока задержки, по другому нельзя достучаться до задачи        
        if (ct.IsCancellationRequested)
        {
            ct.ThrowIfCancellationRequested();
        }
        await FinishAuction(auctionData.AuctionId);
    }

    private async Task FinishAuction(Guid auctionId)
    {
        using (var scope = _services.CreateAsyncScope())
        {
            var _publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            var _dbContext = scope.ServiceProvider.GetRequiredService<EventSourcingDbContext>();
            var _configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var correlationId = Guid.NewGuid();
            using (var transaction = _dbContext.Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var finishedAuctions = _dbContext.set_auction_finished(correlationId, auctionId);
                //возвращаем список записей для изменения соответствующих БД в нужных сервисах
                var listItems = new DataForProcessingServicesList
                {
                    DataObjects = new List<DataForProcessingService>()
                };
                var item = finishedAuctions.FirstOrDefault();
                listItems.DataObjects.Add
                (
                    new DataForProcessingService
                    {
                        DataType = item.entitytype,
                        Data = item.eventdata,
                        CRUD = (CRUD)item.crud
                    }
                );

                _auctionMetrics.FinishAuction();
                var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                    _configuration["CommonAssembly"]).CreateInstance("Common.Contracts.Processing.ESLog_AuctionFinish");
                sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
                sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, listItems);
                await _publishEndpoint.Publish(sendObject);
                await transaction.CommitAsync();
            }
        }
    }

    //получаем список дат окончания всех аукционов
    private List<AuctionFinishedData> GetAuctionsToFinish()
    {
        var finishedList = new List<AuctionFinishedData>();
        using (var scope = _services.CreateAsyncScope())
        {
            var _dbContext = scope.ServiceProvider.GetRequiredService<EventSourcingDbContext>();
            using (var transaction = _dbContext.Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var getAuctionsFinished = _dbContext.get_auction_finished(Guid.NewGuid());

                foreach (var item in getAuctionsFinished.ToList())
                {
                    if (string.IsNullOrEmpty(item.eventdata)) return finishedList;
                    finishedList.Add(JsonSerializer.Deserialize<AuctionFinishedData>(item.eventdata));
                }
                transaction.Commit();
            }
        }
        return finishedList;
    }

    public Task UpdateFinishTasks(Guid auctionId, DateTime auctionEnd, CRUD operationType)
    {
        //отменяем ранее созданную задачу для указанного аукциона
        if (tokens.ContainsKey(auctionId))
        {
            tokens[auctionId].Cancel();
        }

        if (operationType == CRUD.Delete) return Task.CompletedTask;
        //создаем новую задачу (в случае создания или обновления аукциона)
        tokens[auctionId] = new CancellationTokenSource();
        var auction = new AuctionFinishedData
        {
            AuctionId = auctionId,
            AuctionEnd = auctionEnd
        };
        Task.Run(async () =>
        {
            await DelayFinishAuction(tokens[auctionId].Token, auction);
        }, tokens[auctionId].Token);

        return Task.CompletedTask;
    }

}
