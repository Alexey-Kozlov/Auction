using System.Text.Json;
using Common.Contracts.EventSourcing;
using EventSourcingService.Data;
using EventSourcingService.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Services;

public class InsertItemToEventSourcing
{
    private readonly EventSourcingDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public InsertItemToEventSourcing(EventSourcingDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }
    public async Task Processing(ESContract context)
    {
        //доьбавляем новое событие в EventsLog либо завершаем распределенную транзакцию
        using var transaction = _dbContext.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
        switch (context.EntityType)
        {
            case nameof(CommitESOperation):
                //делаем коммит операций данного CorrelationId,
                //тем самым завершая распределенную транзакцию
                var items = await _dbContext.EventsLogs.Where(p => p.CorrelationId == context.CorrelationId).ToListAsync();
                foreach (var item in items)
                {
                    item.Commited = true;
                }
                break;
            default:
                // //если не завершение транзакции - пишем в лог
                // _dbContext.EventsLogs.Add(new EventsLog
                // {
                //     CorrelationId = context.CorrelationId,
                //     CreateAt = DateTime.UtcNow,
                //     Commited = false,
                //     EntityType = context.EntityType,
                //     EventData = JsonDocument.Parse(context.EventData),
                //     UserLogin = context.UserLogin,
                //     AuctionId = context.AuctionId,
                //     OperationType = context.OperationType
                // });
                break;
        }
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}