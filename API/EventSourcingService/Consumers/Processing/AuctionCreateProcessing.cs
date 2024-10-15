using System.Text.Json;
using Common.Contracts;
using EventSourcingService.Data;
using EventSourcingService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Consumers.Processing;

public class AuctionCreateProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _context;
    private readonly ILogger<EventSourcingEventConsumer> _logger;

    public AuctionCreateProcessing(IPublishEndpoint publishEndpoint, EventSourcingDbContext context,
        ILogger<EventSourcingEventConsumer> logger)
    {
        _publishEndpoint = publishEndpoint;
        _context = context;
        _logger = logger;
    }
    public async Task ProcessingCreateAuctionState(ConsumeContext<BaseStateContract> context)
    {
        _context.EventsLogs.Add(new EventsLog
        {
            CommandId = context.Message.CorrelationId,
            CreateAt = DateTime.UtcNow,
            Commited = false,
            Info = context.Message.Type,
            EventData = JsonDocument.Parse(context.Message.Data)
        });
        await _context.SaveChangesAsync();
        await _publishEndpoint.Publish(new AuctionCreated(context.Message.CorrelationId));
        _logger.LogInformation($"Отправлено сообщение AuctionCreated");
    }

    public async Task ProcessingCommitCreateAuctionEvent(ConsumeContext<BaseStateContract> context)
    {
        var item = await _context.EventsLogs.FirstOrDefaultAsync(p => p.CommandId == context.Message.CorrelationId);
        if (item == null)
        {
            throw new Exception($"EventSourcing Item {context.Message.CorrelationId} не найдено");
        }
        item.Commited = true;
        await _context.SaveChangesAsync();
        await _publishEndpoint.Publish(new CommitAuctionCreatedContract(context.Message.CorrelationId));
    }
}