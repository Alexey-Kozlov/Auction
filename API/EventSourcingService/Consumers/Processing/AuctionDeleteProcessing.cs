using System.Text.Json;
using Common.Contracts;
using EventSourcingService.Data;
using EventSourcingService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Consumers.Processing;

public class AuctionDeleteProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _context;

    public AuctionDeleteProcessing(IPublishEndpoint publishEndpoint, EventSourcingDbContext context)
    {
        _publishEndpoint = publishEndpoint;
        _context = context;
    }
    public async Task ProcessingDeleteAuctionState(ConsumeContext<BaseStateContract> context)
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
        await _publishEndpoint.Publish(new AuctionDeleted(context.Message.CorrelationId));
    }

    public async Task ProcessingCommitDeleteAuctionEvent(ConsumeContext<BaseStateContract> context)
    {
        var item = await _context.EventsLogs.FirstOrDefaultAsync(p => p.CommandId == context.Message.CorrelationId);
        if (item == null)
        {
            throw new Exception($"EventSourcing Item {context.Message.CorrelationId} не найдено");
        }
        item.Commited = true;
        await _context.SaveChangesAsync();
        await _publishEndpoint.Publish(new CommitAuctionDeletedContract(context.Message.CorrelationId));
    }
}