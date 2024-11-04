using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Contracts;
using EventSourcingService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

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
                case OperationType.Delete:
                    await AuctionDeleteBidAction(context.Message);
                    break;
            }
        }
    }

    private async Task AuctionBidAction(ESContract context)
    {
        var auctionItem = JsonSerializer.Deserialize<AuctionBidItem>(context.EventData);
        //делаем запись в ES лог об обновлении Entity AuctionBidItem (время окончания аукциона) 
        //или добавлении новой записи в сервисе BiddingService
        await _insertItemToEventSourcing.Processing(context);
        //делаем объект на изменение данных в сервисе BiddingService
        var updateItem = new ActionMessageList<AuctionBidItem>
        {
            ActionItemsList = new List<ActionMessage<AuctionBidItem>>
            {
                new ActionMessage<AuctionBidItem>
                {
                     ActionItem = auctionItem,
                     OperationType = context.OperationType,
                     CorrelationId = context.CorrelationId
                }
            },
            CallBackType = context.CallBackType
        };
        //посылаем в сервис BiddingService для обновления в БД сервиса
        await _publishEndpoint.Publish(updateItem);
    }

    private async Task AuctionDeleteBidAction(ESContract context)
    {
        //Удаление AuctionBidItem и всех BidItem этого аукциона (если они есть)

        var lastSnapShotId = await _dbContext.EventsLogs.Where(p => p.SnapShotId != null)
            .OrderBy(p => p.CreateAt).FirstOrDefaultAsync();
        //получаем из EventSourcing записи по BidItem по данному пользователю, SnapShot
        var eventItems = await _dbContext.EventsLogs.Where(p =>
            (p.SnapShotId == lastSnapShotId.SnapShotId || p.SnapShotId == null) &&
            p.EntityType == nameof(BidItem) &&
            p.AuctionId == context.AuctionId &&
            p.CreateAt >= lastSnapShotId.CreateAt
        ).ToListAsync();
        var deleteAuctionBid = JsonSerializer.Deserialize<AuctionBidItem>(context.EventData);
        //делаем запись в ES лог об удалении AuctionBidItem
        await _insertItemToEventSourcing.Processing(context);
        //объект для отправки в BidService для правки БД
        var complexBidItem = new ComplexAuctionBidItem();
        complexBidItem.auctionBidItem = deleteAuctionBid;
        if (eventItems.Count() > 0)
        {
            //есть BidItem для данного аукциона
            var bidItems = new List<BidItem>();
            foreach (var item in eventItems)
            {
                bidItems.Add(JsonSerializer.Deserialize<BidItem>(item.EventData));
            }
            //делаем записи в ES лог об удалении BidItem данного аукциона
            JsonSerializerOptions options = new()
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            foreach (var bidItem in bidItems)
            {
                context.EventData = JsonSerializer.Serialize(bidItem, bidItem.GetType(), options);
                //пишем в ES лог
                await _insertItemToEventSourcing.Processing(context);
                complexBidItem.bidItems.Add(bidItem);
            }
            //объект для удаления AuctionBidItem а BidService
            var deleteAuctionBidItem = new ActionMessageList<ComplexAuctionBidItem>
            {
                ActionItemsList = new List<ActionMessage<ComplexAuctionBidItem>>
                {
                     new ActionMessage<ComplexAuctionBidItem>
                    {
                        ActionItem = complexBidItem,
                        OperationType = OperationType.Delete,
                        CorrelationId = context.CorrelationId
                    }
                },
                CallBackType = context.CallBackType
            };
            await _publishEndpoint.Publish(deleteAuctionBidItem);
        }
        else
        {
            //нет bids - возвращаемся в state machine
            await _publishEndpoint.Publish(new AuctionDeletedBid { CorrelationId = context.CorrelationId });
        }
    }
}