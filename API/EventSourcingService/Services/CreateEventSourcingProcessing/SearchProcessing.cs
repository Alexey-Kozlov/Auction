using System.Text.Json;
using Common.Contracts;
using EventSourcingService.Data;
using MassTransit;

namespace EventSourcingService.Services.CreateEventSourcingProcessing;

public class SearchProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _dbContext;
    private readonly InsertItemToEventSourcing _insertItemToEventSourcing;

    public SearchProcessing(IPublishEndpoint publishEndpoint, EventSourcingDbContext dbContext,
    InsertItemToEventSourcing insertItemToEventSourcing)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _insertItemToEventSourcing = insertItemToEventSourcing;
    }
    public async Task Processing(ConsumeContext<ESContract> context)
    {
        await AuctionAction(context.Message);
    }

    private async Task AuctionAction(ESContract context)
    {
        //делаем запись в ES об обновлении Entity AuctionItem
        var auctionItem = JsonSerializer.Deserialize<AuctionItem>(context.EventData);
        //записали в ES лог действие
        await _insertItemToEventSourcing.Processing(context);
        //делаем объект на изменение данных в сервисе SearchService
        var updateItem = new ActionMessageList<AuctionItem>
        {
            ActionItemsList = new List<ActionMessage<AuctionItem>>
            {
                new ActionMessage<AuctionItem>
                {
                    ActionItem = auctionItem,
                    OperationType = context.OperationType,
                    CorrelationId = context.CorrelationId
                }
            },
            CallBackType = context.CallBackType
        };
        //посылаем в сервис SearchService для обновления в БД сервиса
        await _publishEndpoint.Publish(updateItem);
    }
}