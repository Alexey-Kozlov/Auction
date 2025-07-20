using System.Reflection;
using AuctionService.Metrics;
using Common.Contracts.Processing;
using Common.Contracts.EventSourcing;
using EventSourcingService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Collections.Concurrent;
using Common.Utils.Logging;
using Common.Contracts.Auction;

namespace EventSourcingService.Services;

public class CheckAuctionFinished : IHostedService, IDisposable
{
    private readonly IServiceProvider _services;
    private readonly AuctionMetrics _auctionMetrics;
    private ConcurrentDictionary<Guid, CancellationTokenSource> tokens = new();
    private Timer _timer;
    public bool IsRunning { get; set; }

    public CheckAuctionFinished(IServiceProvider services, AuctionMetrics auctionMetrics)
    {
        _services = services;
        _auctionMetrics = auctionMetrics;
    }

    private void DoWork()
    {
        //получаем даты, когда должны завершиться аукционы
        //заполняем список задач на завершение и запускаем задания на завершение 
        //при наступлении даты завершения аукциона
        try
        {
            IsRunning = true;
            Console.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " - Background Service is started");
            foreach (var auction in GetAuctionsToFinish())
            {
                var cancelTokenSource = new CancellationTokenSource();
                tokens[auction.ItemId.Value] = cancelTokenSource;
                Task.Run(async () =>
                {
                    await DelayFinishAuction(tokens[auction.ItemId.Value].Token, auction);
                }, tokens[auction.ItemId.Value].Token);
            }
        }
        catch (Exception ex)
        {
            IsRunning = false;
            Console.WriteLine("Background Service Error {0}", ex.Message);
            throw;
        }

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
        await FinishAuction(auctionData);
    }

    private async Task FinishAuction(AuctionFinishedData auctionData)
    {
        using (var scope = _services.CreateAsyncScope())
        {
            var _publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            var _dbContext = scope.ServiceProvider.GetRequiredService<EventSourcingDbContext>();
            var _configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var correlationId = Guid.NewGuid();
            try
            {
                var finishedItem = await _dbContext.set_auction_finished(correlationId, auctionData.ItemId.Value).FirstOrDefaultAsync();
                //возвращаем список записей для изменения соответствующих БД в нужных сервисах
                var listItems = new DataForProcessingServicesList
                {
                    DataObjects = new List<DataForProcessingService>()
                };
                if (finishedItem != null)
                {
                    listItems.DataObjects.Add
                    (
                        new DataForProcessingService
                        {
                            DataType = finishedItem.entitytype,
                            Data = finishedItem.eventdata,
                            CRUD = (CRUD)finishedItem.crud
                        }
                    );
                }
                _auctionMetrics.FinishAuction();
                var itemId = JsonSerializer.Deserialize<AuctionItem>(finishedItem.eventdata).ItemId;
                var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                    _configuration["CommonAssembly"]).CreateInstance("Common.Contracts.Processing.ESLogAuctionFinish");
                sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
                sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, listItems);
                sendObject.GetType().GetProperty("ItemId").SetValue(sendObject, itemId);
                await _publishEndpoint.Publish(sendObject);
            }
            catch (Exception e)
            {
                //ошибки прочие
                var messageObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                    _configuration["CommonAssembly"]).CreateInstance("Common.Contracts.Processing.ESLogAuctionFinish");
                messageObject.GetType().GetProperty("CorrelationId").SetValue(messageObject, correlationId);
                messageObject.GetType().GetProperty("ErrorMessage").SetValue(messageObject, GetErrorMessage.GetInnerException(e).Message);
                messageObject.GetType().GetProperty("ErrorExceptionMessage").SetValue(messageObject, e.StackTrace);
                messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "EventSourcingService_CheckAuctionFinish");
                messageObject.GetType().GetProperty("UserLogin").SetValue(messageObject, "SystemService");
                messageObject.GetType().GetProperty("ItemId").SetValue(messageObject, auctionData.ItemId.Value);
                messageObject.GetType().GetProperty("IsError").SetValue(messageObject, true);

                var faultType = typeof(FaultMessage<>);
                var typeParams = new Type[] { messageObject.GetType() };
                var faultObjectType = faultType.MakeGenericType(typeParams);

                var faultObject = Activator.CreateInstance(faultObjectType, new object[] { messageObject });
                await _publishEndpoint.Publish(faultObject.GetType().GetMethod("CastItem").Invoke(faultObject, null));
            }
        }
    }

    //получаем список дат окончания всех активных (не завершенных) аукционов
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

    public Task UpdateFinishTasks(Guid itemId, DateTime auctionEnd, CRUD operationType)
    {
        //отменяем ранее созданную задачу для указанного аукциона
        if (tokens.ContainsKey(itemId))
        {
            tokens[itemId].Cancel();
        }

        if (operationType == CRUD.Delete) return Task.CompletedTask;
        //создаем новую задачу (в случае создания или обновления аукциона)
        tokens[itemId] = new CancellationTokenSource();
        var auction = new AuctionFinishedData
        {
            ItemId = itemId,
            AuctionEnd = auctionEnd
        };
        Task.Run(async () =>
        {
            await DelayFinishAuction(tokens[itemId].Token, auction);
        }, tokens[itemId].Token);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_timer != null) _timer.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " - BackService is Starting... ");
        //как вариант - периодический запуск метода, например каждую минуту
        //_timer = new Timer(DoWorkAsync, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        //здесь - просто заглушка
        _timer = new Timer(Fake);
        //рабочий метод
        DoWork();
        return Task.CompletedTask;
    }
    //заглушка
    private void Fake(object state) { }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        IsRunning = false;
        Console.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " - BackService is Stopping... ");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }
}
