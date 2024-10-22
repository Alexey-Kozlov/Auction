using System.Text.Json;
using Common.Contracts;
using EventSourcingService.Data;
using EventSourcingService.Entities;
using EventSourcingService.Services;
using MassTransit;

namespace SearchService.Consumers;

public class DbSnapShotConsumer : IConsumer<SendToSetSnapShot>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<DbSnapShotConsumer> _logger;
    private readonly EventSourcingDbContext _context;
    private static readonly AwaitLocker _locker = new AwaitLocker();

    public DbSnapShotConsumer(IPublishEndpoint publishEndpoint,
        ILogger<DbSnapShotConsumer> logger, EventSourcingDbContext context)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
        _context = context;
    }
    public async Task Consume(ConsumeContext<SendToSetSnapShot> consumeContext)
    {
        await _locker.LockAsync(async () =>
        {
            var i = 0;
            foreach (var item in consumeContext.Message.SnapShotItems)
            {
                i++;
                _context.EventsLogs.Add(new EventsLog
                {
                    CorrelationId = Guid.NewGuid(),
                    CreateAt = consumeContext.Message.CreateAt,
                    Commited = true,
                    Info = consumeContext.Message.ProjectName,
                    EventData = JsonDocument.Parse(item),
                    SnapShotId = consumeContext.Message.CorrelationId,
                    TypeOf = consumeContext.Message.ItemsType
                });
            }
            await _context.SaveChangesAsync();
            _logger.LogInformation($"--> Получение сообщения - произвести первоначальную инициализацию записей в БД, записано - {i} записей");
            await _publishEndpoint.Publish(new EventSourcingInitialized($"Произведена запись текущего состояния БД в EventSourcing, сохранено - {i} записей",
                    consumeContext.Message.CorrelationId, consumeContext.Message.UserLogin, consumeContext.Message.SessionId));
        });
    }
}
