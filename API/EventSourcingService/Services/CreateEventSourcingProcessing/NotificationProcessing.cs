using System.Text.Json;
using Common.Contracts;
using EventSourcingService.Data;
using MassTransit;

namespace EventSourcingService.Services.CreateEventSourcingProcessing;

public class NotificationProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _context;
    private readonly InsertItemToEventSourcing _insertItemToEventSourcing;

    public NotificationProcessing(IPublishEndpoint publishEndpoint, EventSourcingDbContext context,
        InsertItemToEventSourcing insertItemToEventSourcing)
    {
        _publishEndpoint = publishEndpoint;
        _context = context;
        _insertItemToEventSourcing = insertItemToEventSourcing;
    }
    public async Task Processing(ConsumeContext<ESContract> context)
    {
        switch (context.Message.OperationType)
        {
            case OperationType.Insert:
                await NotificationAction(context.Message);
                break;
        }
    }

    private async Task NotificationAction(ESContract context)
    {
        //делаем запись в ES о добавлении уведомления NotificationAction в сервисе NotificationService
        var notifyItem = JsonSerializer.Deserialize<NotifyItem>(context.EventData);
        await _insertItemToEventSourcing.Processing(context);
        //делаем объект на добавление уведомления в сервис NotificationService
        var insertItem = new ActionMessageList<NotifyItem>
        (
            new List<ActionMessage<NotifyItem>>
            {
                new ActionMessage<NotifyItem>
                (
                    notifyItem,
                    context.OperationType,
                    context.CorrelationId
                )
            },
            context.CallBackType
        );
        //посылаем в сервис NotificationService для вставки в БД сервиса
        await _publishEndpoint.Publish(insertItem);
    }
}