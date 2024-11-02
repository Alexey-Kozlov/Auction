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

        switch (context.Message.OperationType)
        {
            case OperationType.Update:
            case OperationType.Insert:
                await AuctionAction(context.Message);
                break;
            //ProcessingService -> Activities -> AuctionDelete -> BidActivity
            case OperationType.Delete:
                await _publishEndpoint.Publish(
                    JsonSerializer.Deserialize<AuctionDeletingBid>(context.Message.EventData)
                );
                break;
        }

    }

    private async Task AuctionAction(ESContract context)
    {
        //делаем запись в ES об обновлении Entity AuctionItem
        var auctionItem = JsonSerializer.Deserialize<AuctionItem>(context.EventData);
        //записали в ES запись об обновлении даты окончания аукциона
        await _insertItemToEventSourcing.Processing(context);
        //делаем объект на обновление даты окончания AuctionItem в сервисе SearchService
        var updateItem = new ActionMessageList<AuctionItem>
        (
            new List<ActionMessage<AuctionItem>>
            {
                new ActionMessage<AuctionItem>
                (
                    auctionItem,
                    context.OperationType,
                    context.CorrelationId
                )
            },
            context.CallBackType
        );
        //посылаем в сервис SearchService для обновления в БД сервиса
        await _publishEndpoint.Publish(updateItem);
    }
}