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

    public AuctionCreateProcessing(IPublishEndpoint publishEndpoint, EventSourcingDbContext context)
    {
        _publishEndpoint = publishEndpoint;
        _context = context;
    }
    public async Task ProcessingCreateAuctionState(ConsumeContext<BaseStateContract> context)
    {
        _context.EventsLogs.Add(new EventsLog
        {
            CorrelationId = context.Message.CorrelationId,
            CreateAt = DateTime.UtcNow,
            Commited = false,
            Info = context.Message.Type,
            EventData = JsonDocument.Parse(context.Message.Data)
        });
        await _context.SaveChangesAsync();
        await _publishEndpoint.Publish(new AuctionCreated(context.Message.CorrelationId));
    }

    public async Task ProcessingCommitCreateAuctionEvent(ConsumeContext<BaseStateContract> context)
    {
        using var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
        try
        {
            var item = await _context.EventsLogs.FirstOrDefaultAsync(p => p.CorrelationId == context.Message.CorrelationId);
            if (item == null)
            {
                throw new Exception($"EventSourcing Item {context.Message.CorrelationId} не найдено");
            }
            item.Commited = true;
            await _context.SaveChangesAsync();
            await _publishEndpoint.Publish(new CommitAuctionCreatedContract(context.Message.CorrelationId));
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw new Exception($"Ошибка размещения заявки - {e.Message}");
        }
    }
}