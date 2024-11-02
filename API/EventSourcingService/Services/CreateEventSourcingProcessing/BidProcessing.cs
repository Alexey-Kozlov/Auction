using System.Text.Json;
using Common.Contracts;
using EventSourcingService.Data;
using MassTransit;

namespace EventSourcingService.Services.CreateEventSourcingProcessing;

public class BidProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _dbContext;
    private readonly InsertItemToEventSourcing _insertItemToEventSourcing;

    public BidProcessing(IPublishEndpoint publishEndpoint, EventSourcingDbContext dbContext,
        InsertItemToEventSourcing insertItemToEventSourcing)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _insertItemToEventSourcing = insertItemToEventSourcing;
    }
    public async Task Processing(ConsumeContext<ESContract> context)
    {
        if (context.Message.EntityType == nameof(AuctionBidItem))
        {
            switch (context.Message.OperationType)
            {
                case OperationType.Update:
                case OperationType.Insert:
                    await AuctionBidAction(context.Message);
                    break;
                //ProcessingService -> Activities -> AuctionDelete -> BidActivity
                case OperationType.Delete:
                    await _publishEndpoint.Publish(
                        JsonSerializer.Deserialize<AuctionDeletingBid>(context.Message.EventData)
                    );
                    break;
            }
        }
    }

    private async Task AuctionBidAction(ESContract context)
    {
        //делаем запись в ES об обновлении Entity AuctionBidItem (время окончания аукциона) в сервисе BiddingService
        var auctionItem = JsonSerializer.Deserialize<AuctionBidItem>(context.EventData);
        //записали в ES запись об обновлении даты окончания аукциона
        await _insertItemToEventSourcing.Processing(context);
        //делаем объект на обновление даты окончания AuctionBidItem в сервисе BiddingService
        var updateItem = new ActionMessageList<AuctionBidItem>
        (
            new List<ActionMessage<AuctionBidItem>>
            {
                new ActionMessage<AuctionBidItem>
                (
                    auctionItem,
                    context.OperationType,
                    context.CorrelationId
                )
            },
            context.CallBackType
        );
        //посылаем в сервис BiddingService для обновления в БД сервиса
        await _publishEndpoint.Publish(updateItem);
    }




}