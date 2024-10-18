using System.Text.Json;
using Common.Contracts;
using EventSourcingService.Data;
using EventSourcingService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Consumers.Processing;

public class CommitErrorProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _context;

    public CommitErrorProcessing(IPublishEndpoint publishEndpoint, EventSourcingDbContext context)
    {
        _publishEndpoint = publishEndpoint;
        _context = context;
    }

    public async Task ProcessingCommitErrorEvent(ConsumeContext<BaseStateContract> context)
    {
        using var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
        try
        {
            var item = await _context.EventsLogs.FirstOrDefaultAsync(p => p.CommandId == context.Message.CorrelationId);
            if (item == null)
            {
                throw new Exception($"EventSourcing Item {context.Message.CorrelationId} не найдено");
            }
            item.Info += $", Ошибка -  {context.Message.Data}";
            await _context.SaveChangesAsync();
            //await _publishEndpoint.Publish(new CommitErrorSavedContract(context.Message.CorrelationId));
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
        }
    }
}