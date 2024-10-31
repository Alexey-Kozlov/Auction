using System.Text.Json;
using Common.Contracts;
using EventSourcingService.Data;
using EventSourcingService.Entities;
using EventSourcingService.Services;
using MassTransit;

namespace SearchService.Consumers;

public class CreateDbSnapShotConsumer : IConsumer<SendToSetSnapShot>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CreateDbSnapShotConsumer> _logger;
    private readonly EventSourcingDbContext _context;
    private static readonly AwaitLocker _locker = new AwaitLocker();
    private readonly IConfiguration _configuration;

    public CreateDbSnapShotConsumer(IPublishEndpoint publishEndpoint, IConfiguration configuration,
        ILogger<CreateDbSnapShotConsumer> logger, EventSourcingDbContext context)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
        _context = context;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<SendToSetSnapShot> consumeContext)
    {
        await _locker.LockAsync(async () =>
        {
            var i = 0;
            foreach (var item in consumeContext.Message.SnapShotItems)
            {
                i++;
                var document = JsonDocument.Parse(item);
                var jsonElement = new JsonElement();
                Guid? auctionId = null;
                string userLogin = "";

                try
                {
                    //получаем пользователя - инициатора события
                    switch (consumeContext.Message.ServiceName)
                    {
                        case var val when val == _configuration["ServicesName:BiddingService"]:
                            if (!document.RootElement.TryGetProperty("Bidder", out jsonElement))
                            {
                                document.RootElement.TryGetProperty("Seller", out jsonElement);
                            }
                            break;
                        case var val when val == _configuration["ServicesName:SearchService"]:
                            document.RootElement.TryGetProperty("Seller", out jsonElement);
                            break;
                        case var val when val == _configuration["ServicesName:FinanceService"]:
                        case var val1 when val1 == _configuration["ServicesName:NotificationService"]:
                            document.RootElement.TryGetProperty("UserLogin", out jsonElement);
                            break;
                    }
                    userLogin = jsonElement.GetString();
                    //получаем Id аукциона
                    if (document.RootElement.TryGetProperty("AuctionId", out jsonElement))
                    {
                        auctionId = jsonElement.GetGuid();
                    }
                }
                catch { }
                _context.EventsLogs.Add(new EventsLog
                {
                    CorrelationId = Guid.NewGuid(),
                    CreateAt = consumeContext.Message.CreateAt,
                    Commited = true,
                    ServiceName = consumeContext.Message.ServiceName,
                    EventData = JsonDocument.Parse(item),
                    SnapShotId = consumeContext.Message.CorrelationId,
                    EntityType = consumeContext.Message.ItemsType,
                    RestoringOrder = consumeContext.Message.RestoringOrder,
                    LogicVersion = int.Parse(_configuration["LogicVersion"]),
                    UserLogin = userLogin,
                    AuctionId = auctionId,
                    OperationType = OperationType.Insert
                });
            }
            await _context.SaveChangesAsync();
            _logger.LogInformation($"{DateTime.Now} --> Получение сообщения - произвести первоначальную инициализацию записей в БД, записано - {i} записей");
            await _publishEndpoint.Publish(new EventSourcingInitialized($"Произведена запись текущего состояния БД {consumeContext.Message.ServiceName}" +
            $" в EventSourcing, сохранено - {i} записей",
                    consumeContext.Message.CorrelationId, consumeContext.Message.UserLogin, consumeContext.Message.SessionId));
        });
    }
}
