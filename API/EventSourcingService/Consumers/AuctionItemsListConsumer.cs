using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Contracts;
using EventSourcingService.Data;
using EventSourcingService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;


namespace SearchService.Consumers;

public class AuctionItemsListConsumer : IConsumer<AuctionItemsList>
{
    private readonly ILogger<AuctionItemsListConsumer> _logger;
    private readonly EventSourcingDbContext _context;

    public AuctionItemsListConsumer(EventSourcingDbContext context,
        ILogger<AuctionItemsListConsumer> logger)
    {
        _logger = logger;
        _context = context;
    }
    public async Task Consume(ConsumeContext<AuctionItemsList> consumeContext)
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
            if (await _context.EventsLogs.FirstOrDefaultAsync(p => p.CommandId == item.Id) != null)
            {
                continue;
            }
            i++;

            _context.EventsLogs.Add(new EventsLog
            {
                CommandId = item.Id,
                CreateAt = item.CreateAt,
                Commited = true,
                Info = "Первоначальная инициализация записей",
                EventData = JsonDocument.Parse(JsonSerializer.Serialize(item, item.GetType(), options))
            });
        }
        await _context.SaveChangesAsync();
        _logger.LogInformation($"--> Получение сообщения - произвести первоначальную инициализацию записей в БД, записано - {i} записей");
    }
}
