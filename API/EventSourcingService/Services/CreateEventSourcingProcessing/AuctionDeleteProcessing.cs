using Common.Contracts.EventSourcing;
using EventSourcingService.Data;
using MassTransit;

namespace EventSourcingService.Services.CreateEventSourcingProcessing;

public class AuctionDeleteProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _dbContext;
    private readonly InsertItemToEventSourcing _insertItemToEventSourcing;

    public AuctionDeleteProcessing(IPublishEndpoint publishEndpoint, InsertItemToEventSourcing insertItemToEventSourcing,
        EventSourcingDbContext dbContext)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _insertItemToEventSourcing = insertItemToEventSourcing;
    }
    public async Task Processing(ConsumeContext<ESContract> context)
    {

    }

    // private async Task MakeBid(ESContract context)
    // {
    //     try
    //     {
    //         /*запускаем обработку, выполняется:
    //         - корректирующая запись по отмене прежней ставки (платежа ро ней), если была        
    //         - расчет баланса для текущего пользователя
    //         - если ставка превышает баланс - исключение с откатом всех изменений
    //         - пишем в лог списание денег на новую ставку
    //         - возвращаются: запись с текущим балансом (Status=2) и запись для отмены платежа (Status=0, если есть)
    //         */
    //         //получили баланс
    //         var result = await _dbContext.placebid(
    //             context.CorrelationId,
    //             context.AuctionId ?? Guid.NewGuid(),
    //             context.EventData,
    //             context.UserLogin).ToListAsync();

    //         //посылаем в FinanceService для обновления БД
    //         var sendFinanceItem = new ActionMessageList<FinanceItem>
    //         {
    //             ActionItemsList = new List<ActionMessage<FinanceItem>>(),
    //             CallBackType = context.CallBackType
    //         };
    //         foreach (var finItem in result)
    //         {
    //             if (finItem.status == FinanceRecordStatus.Баланс)
    //             {
    //                 //записи для правки баланса финансов
    //                 sendFinanceItem.ActionItemsList.Add(
    //                     new ActionMessage<FinanceItem>
    //                     {
    //                         ActionItem = new FinanceItem
    //                         {
    //                             ActionDate = DateTime.UtcNow,
    //                             AuctionId = context.AuctionId,
    //                             Id = Guid.NewGuid(),
    //                             Status = FinanceRecordStatus.Баланс,
    //                             UserLogin = finItem.userlogin,
    //                             Value = finItem.value
    //                         },
    //                         OperationType = OperationType.Update,
    //                         CorrelationId = context.CorrelationId
    //                     }
    //                 );
    //             }
    //             else
    //             {
    //                 //записи для создания/удаления записи о ставке
    //                 sendFinanceItem.ActionItemsList.Add(
    //                     new ActionMessage<FinanceItem>
    //                     {
    //                         ActionItem = new FinanceItem
    //                         {
    //                             ActionDate = DateTime.UtcNow,
    //                             AuctionId = context.AuctionId,
    //                             Id = Guid.NewGuid(),
    //                             Status = finItem.status,
    //                             UserLogin = finItem.userlogin,
    //                             Value = finItem.value
    //                         },
    //                         OperationType = finItem.status == FinanceRecordStatus.Приход ?
    //                             OperationType.Delete : OperationType.Insert,
    //                         CorrelationId = context.CorrelationId
    //                     }
    //                 );
    //             }

    //         };

    //         await _publishEndpoint.Publish(sendFinanceItem);
    //     }
    //     catch (Npgsql.PostgresException e)
    //     {
    //         //ошибка при выполнении транзакции в БД, в т.ч. штатные - при нехватке денег на ставку
    //         Fault<BidFinanceGranted> per = new FaultMessage<BidFinanceGranted>(
    //             e.MessageText,
    //             new BidFinanceGranted { CorrelationId = context.CorrelationId }
    //         );
    //         await _publishEndpoint.Publish(per);
    //     }
    //     catch (Exception e)
    //     {
    //         //все остальные ошибки программы
    //         Fault<BidFinanceGranted> per = new FaultMessage<BidFinanceGranted>(
    //             e.Message,
    //             new BidFinanceGranted { CorrelationId = context.CorrelationId }
    //         );
    //         await _publishEndpoint.Publish(per);
    //     }
    // }

    // private async Task FinanceCreate(ESContract context)
    // {
    //     /*запускаем обработку, выполняется:     
    //     - расчет баланса для текущего пользователя
    //     - пишем в лог приход денег
    //     - возвращаются: запись с текущим балансом (Status=2)
    //     */
    //     //получили баланс
    //     var result = await _dbContext.finance_create(
    //         context.CorrelationId,
    //         context.AuctionId ?? Guid.NewGuid(),
    //         context.EventData,
    //         context.UserLogin).ToListAsync();

    //     var balance = result.FirstOrDefault(p => p.status == FinanceRecordStatus.Баланс);

    //     //посылаем в FinanceService для обновления БД
    //     var sendFinanceItem = new ActionMessageList<FinanceItem>
    //     {
    //         ActionItemsList = new List<ActionMessage<FinanceItem>>
    //             {
    //                 //объект для создания платежа а FinanceService
    //                 new ActionMessage<FinanceItem>
    //                 {
    //                     ActionItem = JsonSerializer.Deserialize<FinanceItem>(context.EventData),
    //                     OperationType = OperationType.Insert,
    //                     CorrelationId = context.CorrelationId
    //                 },
    //                 //объект для обновления баланса а FinanceService
    //                 new ActionMessage<FinanceItem>
    //                 {
    //                     ActionItem = new FinanceItem
    //                     {
    //                         ActionDate = DateTime.UtcNow,
    //                         AuctionId = context.AuctionId,
    //                         Id = Guid.NewGuid(),
    //                         Status = FinanceRecordStatus.Баланс,
    //                         UserLogin = balance.userlogin,
    //                         Value = balance.value
    //                     },
    //                     OperationType = OperationType.Update,
    //                     CorrelationId = context.CorrelationId
    //                 }
    //             },
    //         CallBackType = context.CallBackType
    //     };
    //     await _publishEndpoint.Publish(sendFinanceItem);
    // }

    // private async Task FinanceDelete(ESContract context)
    // {
    //     //крайний SnapShot
    //     var lastSnapShotId = await _dbContext.EventsLogs.Where(p => p.SnapShotId != null)
    //         .OrderBy(p => p.CreateAt).FirstOrDefaultAsync();
    //     //получаем из EventSourcing записи по деньгам по данному пользователю, SnapShot
    //     var financeItems = await _dbContext.EventsLogs.Where(p =>
    //         (p.SnapShotId == lastSnapShotId.SnapShotId || p.SnapShotId == null) &&
    //         p.EntityType == nameof(FinanceItem) &&
    //         p.UserLogin == context.UserLogin &&
    //         p.CreateAt >= lastSnapShotId.CreateAt
    //     ).ToListAsync();
    //     //если были ставки по данному аукциону - то были списаны деньги
    //     var auctionItem = financeItems.FirstOrDefault(p => p.AuctionId == context.AuctionId);
    //     if (auctionItem != null)
    //     {
    //         //делаем запись для удаления из финансов, пишем в ES
    //         JsonSerializerOptions options = new()
    //         {
    //             ReferenceHandler = ReferenceHandler.IgnoreCycles,
    //             WriteIndented = true,
    //             Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    //         };
    //         var deleteItem = JsonSerializer.Deserialize<FinanceItem>(auctionItem.EventData);
    //         deleteItem.ActionDate = DateTime.UtcNow;
    //         context.EventData = JsonSerializer.Serialize(deleteItem, deleteItem.GetType(), options);
    //         //пишем в ES лог
    //         await _insertItemToEventSourcing.Processing(context);
    //         var balance = GetBalance(financeItems, deleteItem.Value);
    //         balance.UserLogin = context.UserLogin;
    //         var sendFinanceItem = new ActionMessageList<FinanceItem>
    //         {
    //             ActionItemsList = new List<ActionMessage<FinanceItem>>
    //             {
    //                 //объект для удаления платежа а FinanceService
    //                 new ActionMessage<FinanceItem>
    //                 {
    //                     ActionItem = deleteItem,
    //                     OperationType = OperationType.Delete,
    //                     CorrelationId = context.CorrelationId
    //                 },
    //                 //объект для обновления баланса а FinanceService
    //                 new ActionMessage<FinanceItem>
    //                 {
    //                     ActionItem = balance,
    //                     OperationType = OperationType.Update,
    //                     CorrelationId = context.CorrelationId
    //                 }
    //             },
    //             CallBackType = context.CallBackType
    //         };
    //         await _publishEndpoint.Publish(sendFinanceItem);
    //     }
    //     else
    //     {
    //         //нет платежей - возвращаемся в state machine
    //         await _publishEndpoint.Publish(new AuctionDeletedFinance { CorrelationId = context.CorrelationId });
    //     }

    // }

    // {
    //     var financeItems = new List<FinanceItem>();
    //     foreach (var item in items)
    //     {
    //         financeItems.Add(JsonSerializer.Deserialize<FinanceItem>(item.EventData));
    //     }
    //     //получаем сумму средита
    //     int? credit = financeItems.Where(p => p.Status == FinanceRecordStatus.Приход).Sum(p => p.Value);
    // //получаем сумму дебита
    // int? debit = financeItems.Where(p => p.Status == FinanceRecordStatus.Расход).Sum(p => p.Value);
    // //баланс, добавляем деньги от удаленного аукциона
    // var balance = (credit ?? 0) - (debit ?? 0) + correctValue;

    // //делаем объект на обновление баланса
    // var balanceItem = new FinanceItem();
    //     balanceItem.Id = Guid.NewGuid();
    //     balanceItem.Value = balance;
    //     balanceItem.Status = FinanceRecordStatus.Баланс;
    //     balanceItem.ActionDate = DateTime.UtcNow;
    //     return balanceItem;
    // }
}

