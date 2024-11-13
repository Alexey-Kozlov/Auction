using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Contracts;
using EventSourcingService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Services.CreateEventSourcingProcessing;

public class BidProcessing
{
    // private readonly IPublishEndpoint _publishEndpoint;
    // private readonly EventSourcingDbContext _dbContext;
    // private readonly InsertItemToEventSourcing _insertItemToEventSourcing;

    // public BidProcessing(IPublishEndpoint publishEndpoint, EventSourcingDbContext dbContext,
    //     InsertItemToEventSourcing insertItemToEventSourcing)
    // {
    //     _publishEndpoint = publishEndpoint;
    //     _dbContext = dbContext;
    //     _insertItemToEventSourcing = insertItemToEventSourcing;
    // }
    // public async Task Processing(ConsumeContext<ESContract> context)
    // {
    //     //делаем операции с аукционом - создание, изменение, удаление
    //     if (context.Message.EntityType == nameof(AuctionBidItem))
    //     {
    //         switch (context.Message.OperationType)
    //         {
    //             case OperationType.Update:
    //             case OperationType.Insert:
    //                 await AuctionBidAction(context.Message);
    //                 break;
    //             case OperationType.Delete:
    //                 await AuctionDeleteBidAction(context.Message);
    //                 break;
    //         }
    //     }
    //     //делаем ставку
    //     if (context.Message.EntityType == nameof(BidItem))
    //     {
    //         await BidAction(context.Message);
    //     }
    // }

    // private async Task BidAction(ESContract context)
    // {
    //     var bidItem = JsonSerializer.Deserialize<BidItem>(context.EventData);

    //     //проверки на наличие связанного аукциона объектов и наличие более высоких ставок
    //     var lastSnapShotId = await _dbContext.EventsLogs.Where(p => p.SnapShotId != null)
    //         .OrderBy(p => p.CreateAt).FirstOrDefaultAsync();
    //     //получаем из EventSourcing записи по BidItem по данному пользователю, SnapShot
    //     var auctionBidItem = await _dbContext.EventsLogs.Where(p =>
    //         (p.SnapShotId == lastSnapShotId.SnapShotId || p.SnapShotId == null) &&
    //         p.EntityType == nameof(AuctionBidItem) &&
    //         p.AuctionId == context.AuctionId &&
    //         p.CreateAt >= lastSnapShotId.CreateAt
    //     ).OrderByDescending(p => p.Version).FirstOrDefaultAsync();
    //     if (auctionBidItem == null || auctionBidItem.OperationType == OperationType.Delete)
    //     {
    //         Console.WriteLine($"{DateTime.Now} Ошибка обновления записи - запись аукциона " + context.AuctionId + " не найдена.");
    //         throw new Exception($"{DateTime.Now} Ошибка обновления записи - запись аукциона " + context.AuctionId + " не найдена.");
    //     }
    //     var bids = await _dbContext.EventsLogs.Where(p =>
    //         (p.SnapShotId == lastSnapShotId.SnapShotId || p.SnapShotId == null) &&
    //         p.EntityType == nameof(BidItem) &&
    //         p.AuctionId == context.AuctionId &&
    //         p.CreateAt >= lastSnapShotId.CreateAt
    //     ).Select(p => p.EventData).ToListAsync();
    //     var bidItems = new List<BidItem>();
    //     bidItems.AddRange(bids.Select(p => p.Deserialize<BidItem>()).OrderByDescending(p => p.BidTime));
    //     if (bidItems.Any() && bidItems[0].Amount >= bidItem.Amount)
    //     {
    //         Console.WriteLine($"{DateTime.Now} Ошибка обновления записи - запись аукциона " + context.AuctionId + " не найдена.");
    //         throw new Exception($"Ошибка новой ставки - ставка {bidItem.Amount} меньше или равна существующей ставке - {bidItems[0].Amount}");
    //     }
    //     //делаем запись в ES лог о создании ставки
    //     await _insertItemToEventSourcing.Processing(context);
    //     //делаем объект на изменение данных в сервисе BiddingService
    //     var updateItem = new ActionMessageList<BidItem>
    //     {
    //         ActionItemsList = new List<ActionMessage<BidItem>>
    //         {
    //             new ActionMessage<BidItem>
    //             {
    //                  ActionItem = bidItem,
    //                  OperationType = context.OperationType,
    //                  CorrelationId = context.CorrelationId
    //             }
    //         },
    //         CallBackType = context.CallBackType
    //     };
    //     //посылаем в сервис BiddingService для обновления в БД сервиса
    //     await _publishEndpoint.Publish(updateItem);
    // }

    // private async Task AuctionBidAction(ESContract context)
    // {
    //     var auctionItem = JsonSerializer.Deserialize<AuctionBidItem>(context.EventData);
    //     //делаем запись в ES лог об обновлении Entity AuctionBidItem (время окончания аукциона) 
    //     //или добавлении новой записи в сервисе BiddingService
    //     await _insertItemToEventSourcing.Processing(context);

    //     //проверка - если обновление - есть ли такой объект в ES лог
    //     if (context.OperationType == OperationType.Update)
    //     {
    //         var lastSnapShotId = await _dbContext.EventsLogs.Where(p => p.SnapShotId != null)
    //             .OrderBy(p => p.CreateAt).FirstOrDefaultAsync();
    //         //получаем из EventSourcing записи по BidItem по данному пользователю, SnapShot
    //         var auctionBidItem = await _dbContext.EventsLogs.Where(p =>
    //             (p.SnapShotId == lastSnapShotId.SnapShotId || p.SnapShotId == null) &&
    //             p.EntityType == nameof(AuctionBidItem) &&
    //             p.AuctionId == context.AuctionId &&
    //             p.CreateAt >= lastSnapShotId.CreateAt
    //         ).OrderByDescending(p => p.Version).FirstOrDefaultAsync();
    //         if (auctionBidItem == null || auctionBidItem.OperationType == OperationType.Delete)
    //         {
    //             Console.WriteLine($"{DateTime.Now} Ошибка обновления записи - запись аукциона " + context.AuctionId + " не найдена.");
    //             throw new Exception($"{DateTime.Now} Ошибка обновления записи - запись аукциона " + context.AuctionId + " не найдена.");
    //         }
    //     }

    //     //делаем объект на изменение данных в сервисе BiddingService
    //     var updateItem = new ActionMessageList<AuctionBidItem>
    //     {
    //         ActionItemsList = new List<ActionMessage<AuctionBidItem>>
    //         {
    //             new ActionMessage<AuctionBidItem>
    //             {
    //                  ActionItem = auctionItem,
    //                  OperationType = context.OperationType,
    //                  CorrelationId = context.CorrelationId
    //             }
    //         },
    //         CallBackType = context.CallBackType
    //     };
    //     //посылаем в сервис BiddingService для обновления в БД сервиса
    //     await _publishEndpoint.Publish(updateItem);
    // }

    // private async Task AuctionDeleteBidAction(ESContract context)
    // {
    //     //Удаление AuctionBidItem и всех BidItem этого аукциона (если они есть)

    //     var lastSnapShotId = await _dbContext.EventsLogs.Where(p => p.SnapShotId != null)
    //         .OrderBy(p => p.CreateAt).FirstOrDefaultAsync();
    //     //получаем из EventSourcing записи по BidItem по данному пользователю, SnapShot
    //     var eventItems = await _dbContext.EventsLogs.Where(p =>
    //         (p.SnapShotId == lastSnapShotId.SnapShotId || p.SnapShotId == null) &&
    //         p.EntityType == nameof(BidItem) &&
    //         p.AuctionId == context.AuctionId &&
    //         p.CreateAt >= lastSnapShotId.CreateAt
    //     ).ToListAsync();
    //     var deleteAuctionBid = JsonSerializer.Deserialize<AuctionBidItem>(context.EventData);
    //     //делаем запись в ES лог об удалении AuctionBidItem
    //     await _insertItemToEventSourcing.Processing(context);
    //     //объект для отправки в BidService для правки БД
    //     var complexBidItem = new ComplexAuctionBidItem();
    //     complexBidItem.auctionBidItem = deleteAuctionBid;
    //     if (eventItems.Count() > 0)
    //     {
    //         //есть BidItem для данного аукциона
    //         var bidItems = new List<BidItem>();
    //         foreach (var item in eventItems)
    //         {
    //             bidItems.Add(JsonSerializer.Deserialize<BidItem>(item.EventData));
    //         }
    //         //делаем записи в ES лог об удалении BidItem данного аукциона
    //         JsonSerializerOptions options = new()
    //         {
    //             ReferenceHandler = ReferenceHandler.IgnoreCycles,
    //             WriteIndented = true,
    //             Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    //         };
    //         foreach (var bidItem in bidItems)
    //         {
    //             context.EventData = JsonSerializer.Serialize(bidItem, bidItem.GetType(), options);
    //             //пишем в ES лог
    //             await _insertItemToEventSourcing.Processing(context);
    //             complexBidItem.bidItems.Add(bidItem);
    //         }
    //         //объект для удаления AuctionBidItem а BidService
    //         var deleteAuctionBidItem = new ActionMessageList<ComplexAuctionBidItem>
    //         {
    //             ActionItemsList = new List<ActionMessage<ComplexAuctionBidItem>>
    //             {
    //                  new ActionMessage<ComplexAuctionBidItem>
    //                 {
    //                     ActionItem = complexBidItem,
    //                     OperationType = OperationType.Delete,
    //                     CorrelationId = context.CorrelationId
    //                 }
    //             },
    //             CallBackType = context.CallBackType
    //         };
    //         await _publishEndpoint.Publish(deleteAuctionBidItem);
    //     }
    //     else
    //     {
    //         //нет bids - возвращаемся в state machine
    //         await _publishEndpoint.Publish(new AuctionDeletedBid { CorrelationId = context.CorrelationId });
    //     }
    // }
}