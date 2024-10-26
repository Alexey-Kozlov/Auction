using System.Text.Json;
using Common.Contracts;
using EventSourcingService.Data;
using EventSourcingService.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Services;

public class InsertItemToEventSourcing
{
    private readonly EventSourcingDbContext _context;
    private readonly IConfiguration _configuration;

    public InsertItemToEventSourcing(EventSourcingDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }
    public async Task Processing(BaseStateContract context)
    {

        _context.EventsLogs.Add(new EventsLog
        {
            CorrelationId = context.CorrelationId,
            CreateAt = DateTime.UtcNow,
            Commited = false,
            EntityType = context.EntityType,
            ServiceName = context.ServiceName,
            EventData = JsonDocument.Parse(context.EventData),
            LogicVersion = int.Parse(_configuration["LogicVersion"])
        });
        await _context.SaveChangesAsync();
        if (context.EntityType == nameof(CommitESOperation))
        {
            //при поступлении таких сообщений из Кафки - делаем коммит операций данного CorrelationId
            var items = await _context.EventsLogs.Where(p => p.CorrelationId == context.CorrelationId).ToListAsync();
            foreach (var item in items)
            {
                item.Commited = true;
            }
            await _context.SaveChangesAsync();
        }

    }
}