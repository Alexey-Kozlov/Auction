using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Contracts;
using EventSourcingService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Services.CreateEventSourcingProcessing;

public class NotificationProcessing1
{
    // private readonly IPublishEndpoint _publishEndpoint;
    // private readonly EventSourcingDbContext _dbContext;
    // private readonly InsertItemToEventSourcing _insertItemToEventSourcing;

    // public NotificationProcessing(IPublishEndpoint publishEndpoint, EventSourcingDbContext dbContext,
    //     InsertItemToEventSourcing insertItemToEventSourcing)
    // {
    //     _publishEndpoint = publishEndpoint;
    //     _dbContext = dbContext;
    //     _insertItemToEventSourcing = insertItemToEventSourcing;
    // }
    // public async Task Processing(ConsumeContext<ESContract> context)
    // {
    //     switch (context.Message.OperationType)
    //     {
    //         case OperationType.Insert:
    //             await NotificationInsertAction(context.Message);
    //             break;
    //         case OperationType.Delete:
    //             await NotificationDeleteAction(context.Message);
    //             break;
    //         case OperationType.Update:
    //             await NotificationUpdateAction(context.Message);
    //             break;
    //         case OperationType.Bid:
    //             await BidAction(context.Message);
    //             break;
    //     }
    // }

    // private async Task BidAction(ESContract context)
    // {
    //     var bidItem = JsonSerializer.Deserialize<NotifyItem>(context.EventData);

    //     //проверки на наличие уведомления для этого аукциона.
    //     //Если нет уведомления - делаем
    //     var lastSnapShotId = await _dbContext.EventsLogs.Where(p => p.SnapShotId != null)
    //         .OrderBy(p => p.CreateAt).FirstOrDefaultAsync();
    //     //получаем из EventSourcing записи по NotifyItem по данному пользователю, SnapShot
    //     var auctionBidItem = await _dbContext.EventsLogs.Where(p =>
    //         (p.SnapShotId == lastSnapShotId.SnapShotId || p.SnapShotId == null) &&
    //         p.EntityType == nameof(NotifyItem) &&
    //         p.AuctionId == context.AuctionId &&
    //         p.UserLogin == bidItem.UserLogin &&
    //         p.CreateAt >= lastSnapShotId.CreateAt
    //     ).OrderByDescending(p => p.Version).FirstOrDefaultAsync();

    //     var newNotifyItem = new ActionMessageList<NotifyItem>
    //     {
    //         ActionItemsList = new List<ActionMessage<NotifyItem>>
    //             {
    //                 new ActionMessage<NotifyItem>
    //                 {
    //                     ActionItem = JsonSerializer.Deserialize<NotifyItem>(context.EventData),
    //                     OperationType = OperationType.Bid,
    //                     CorrelationId = context.CorrelationId
    //                 }
    //             },
    //         CallBackType = context.CallBackType,
    //         Properties = new List<string> { "false" }
    //     };
    //     if (auctionBidItem == null || auctionBidItem.OperationType == OperationType.Delete)
    //     {
    //         //уведомления нет - создаем новое в ES лог
    //         await _insertItemToEventSourcing.Processing(context);
    //         //делаем пометку - нужно создать запись в БД о подписке
    //         newNotifyItem.Properties = new List<string> { "true" };
    //     }
    //     //объект для добавления NotifyItem а NotificationService
    //     await _publishEndpoint.Publish(newNotifyItem);
    // }


    // private async Task NotificationDeleteAction(ESContract context)
    // {
    //     var lastSnapShotId = await _dbContext.EventsLogs.Where(p => p.SnapShotId != null)
    //         .OrderBy(p => p.CreateAt).FirstOrDefaultAsync();
    //     //получаем из EventSourcing записи по NotifyItem по данному аукциону, SnapShot
    //     var eventItems = await _dbContext.EventsLogs.Where(p =>
    //         (p.SnapShotId == lastSnapShotId.SnapShotId || p.SnapShotId == null) &&
    //         p.EntityType == nameof(NotifyItem) &&
    //         p.AuctionId == context.AuctionId &&
    //         p.CreateAt >= lastSnapShotId.CreateAt
    //     ).ToListAsync();
    //     //получаем Title аукциона для передачи в сервис уведомлений
    //     var auctionTitleJson = await _dbContext.EventsLogs.Where(p =>
    //         p.AuctionId == context.AuctionId &&
    //         p.OperationType != OperationType.Delete &&
    //         p.EntityType == "AuctionItem").OrderByDescending(p => p.Version).FirstOrDefaultAsync();
    //     var auctionTitle = JsonSerializer.Deserialize<AuctionItem>(auctionTitleJson.EventData);

    //     //если есть уведомления для данного аукциона - делаем объект для отправки в NotifyService для правки БД
    //     if (eventItems.Count() > 0)
    //     {
    //         //есть NotifyItem для данного аукциона
    //         var notifyItems = new List<NotifyItem>();
    //         foreach (var item in eventItems)
    //         {
    //             notifyItems.Add(JsonSerializer.Deserialize<NotifyItem>(item.EventData));
    //         }
    //         JsonSerializerOptions options = new()
    //         {
    //             ReferenceHandler = ReferenceHandler.IgnoreCycles,
    //             WriteIndented = true,
    //             Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    //         };
    //         var listNotifyItem = new List<ActionMessage<NotifyItem>>();
    //         foreach (var notifyItem in notifyItems)
    //         {
    //             context.EventData = JsonSerializer.Serialize(notifyItem, notifyItem.GetType(), options);
    //             //пишем в ES лог
    //             await _insertItemToEventSourcing.Processing(context);
    //             listNotifyItem.Add
    //             (
    //                 new ActionMessage<NotifyItem>
    //                 {
    //                     ActionItem = notifyItem,
    //                     OperationType = OperationType.Delete,
    //                     CorrelationId = context.CorrelationId
    //                 }
    //             );
    //         }
    //         //объект для удаления NotifyItem а NotificationService
    //         var deleteNotifyItem = new ActionMessageList<NotifyItem>
    //         {
    //             ActionItemsList = listNotifyItem,
    //             CallBackType = context.CallBackType,
    //             Properties = new List<string>
    //             {
    //                 auctionTitle.Title
    //             }
    //         };
    //         await _publishEndpoint.Publish(deleteNotifyItem);
    //     }


    // }
    // private async Task NotificationInsertAction(ESContract context)
    // {
    //     //делаем запись в ES о добавлении уведомления NotificationAction в сервисе NotificationService
    //     var notifyItem = JsonSerializer.Deserialize<NotifyItem>(context.EventData);
    //     await _insertItemToEventSourcing.Processing(context);
    //     //получаем Title аукциона для передачи в сервис уведомлений
    //     var auctionItemJson = await _dbContext.EventsLogs.Where(p =>
    //         p.AuctionId == context.AuctionId &&
    //         p.EntityType == "AuctionItem").OrderByDescending(p => p.Version).FirstOrDefaultAsync();
    //     var auctionItem = JsonSerializer.Deserialize<AuctionItem>(auctionItemJson.EventData);
    //     //делаем объект на добавление уведомления в сервис NotificationService

    //     var insertItem = new ActionMessageList<NotifyItem>
    //     {
    //         ActionItemsList = new List<ActionMessage<NotifyItem>>
    //         {
    //             new ActionMessage<NotifyItem>
    //             {
    //                 ActionItem = notifyItem,
    //                 OperationType = context.OperationType,
    //                 CorrelationId = context.CorrelationId
    //             }
    //         },
    //         CallBackType = context.CallBackType,
    //         Properties = new List<string>
    //         {
    //             auctionItem.Title
    //         }
    //     };
    //     //посылаем сообщение в сервис NotificationService для изменения данных
    //     await _publishEndpoint.Publish(insertItem);
    // }
    // private async Task NotificationUpdateAction(ESContract context)
    // {
    //     //получаем Title аукциона для передачи в сервис уведомлений
    //     var auctionItemJson = await _dbContext.EventsLogs.Where(p =>
    //         p.AuctionId == context.AuctionId &&
    //         p.EntityType == "AuctionItem").OrderByDescending(p => p.Version).FirstOrDefaultAsync();
    //     var auctionItem = JsonSerializer.Deserialize<AuctionItem>(auctionItemJson.EventData);
    //     //делаем объект в сервис NotificationService для рассылки оповещения
    //     var messageItem = new ActionMessageList<NotifyItem>
    //     {
    //         ActionItemsList = new List<ActionMessage<NotifyItem>>
    //         {
    //             new ActionMessage<NotifyItem>
    //             {
    //                 ActionItem = JsonSerializer.Deserialize<NotifyItem>(context.EventData),
    //                 OperationType = OperationType.Update,
    //                 CorrelationId = context.CorrelationId
    //             }
    //         },
    //         CallBackType = context.CallBackType,
    //         Properties = new List<string>
    //         {
    //             auctionItem.Title
    //         }
    //     };
    //     //посылаем сообщение в сервис NotificationService
    //     await _publishEndpoint.Publish(messageItem);
    // }
}