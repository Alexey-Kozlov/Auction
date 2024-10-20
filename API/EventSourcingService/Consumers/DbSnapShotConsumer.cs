using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Contracts;
using EventSourcingService.Data;
using EventSourcingService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;


namespace SearchService.Consumers;

public class DbSnapShotConsumer : IConsumer<SendToSetSnapShot>
{
    private readonly ILogger<DbSnapShotConsumer> _logger;
    private readonly EventSourcingDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public DbSnapShotConsumer(EventSourcingDbContext context,
        ILogger<DbSnapShotConsumer> logger, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _context = context;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<SendToSetSnapShot> consumeContext)
    {
        var i = 0;
        JsonSerializerOptions options = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        foreach (var item in consumeContext.Message.AuctionItems)
        {
            //если уже перенесли запись - пропускаем
            if (await _context.EventsLogs.FirstOrDefaultAsync(p => p.CorrelationId == item.Id) != null)
            {
                continue;
            }
            i++;

            _context.EventsLogs.Add(new EventsLog
            {
                CorrelationId = item.Id,
                CreateAt = item.CreateAt,
                Commited = true,
                Info = "Первоначальная инициализация записей",
                EventData = JsonDocument.Parse(JsonSerializer.Serialize(item, item.GetType(), options)),
                SnapShotId = consumeContext.Message.CorrelationId
            });
        }
        await _context.SaveChangesAsync();
        _logger.LogInformation($"--> Получение сообщения - произвести первоначальную инициализацию записей в БД, записано - {i} записей");
        await _publishEndpoint.Publish(new EventSourcingInitialized($"Произведена запись текущего состояния БД в EventSourcing, сохранено - {i} записей",
            consumeContext.Message.CorrelationId, consumeContext.Message.UserLogin, consumeContext.Message.SessionId));
    }
}
