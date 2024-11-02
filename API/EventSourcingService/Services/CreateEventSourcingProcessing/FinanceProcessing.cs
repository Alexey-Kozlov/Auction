using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Contracts;
using EventSourcingService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Services.CreateEventSourcingProcessing;

public class FinanceProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _dbContext;
    private readonly InsertItemToEventSourcing _insertItemToEventSourcing;

    public FinanceProcessing(IPublishEndpoint publishEndpoint, InsertItemToEventSourcing insertItemToEventSourcing,
        EventSourcingDbContext dbContext)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _insertItemToEventSourcing = insertItemToEventSourcing;
    }
    public async Task Processing(ConsumeContext<ESContract> context)
    {
        switch (context.Message.OperationType)
        {
            //ProcessingService -> Activities -> AuctionDelete -> FinanceActivity                         
            case OperationType.Delete:
                //вызываем сервис по корректировке платежей по аукциону
                await FinanceDelete(context.Message);
                break;
            //ProcessingService -> Activities -> Finance -> FinanceActivity                         
            case OperationType.Insert:
                //вызываем сервис по добавлению денег
                await FinanceCreate(context.Message);
                break;
        }
    }

    private async Task FinanceCreate(ESContract context)
    {
        // using var transaction = _dbContext.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
        // //делаем объект по обновлению балансабаланс
        // var updateBalance = await GetBalance(context);

        // //делаем объект для добавления денег
        // var addFinance = new FinanceCreateMessage(JsonSerializer.Deserialize<FinanceItem>(context.EventData),
        //      OperationType.Insert, context.CorrelationId);

        // await _dbContext.SaveChangesAsync();
        // await transaction.CommitAsync();
        // var financeToDo = new List<FinanceCreateMessage>
        // {
        //     addFinance,
        //     updateBalance
        // };
        // await _publishEndpoint.Publish(new FinanceAddCredit(financeToDo));
    }

    private async Task FinanceDelete(ESContract context)
    {
        using var transaction = _dbContext.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
        var deleteItem = JsonSerializer.Deserialize<FinanceItem>(context.EventData);
        //крайний SnapShot
        var lastSnapShotId = _dbContext.EventsLogs.Where(p => p.SnapShotId != null)
            .OrderBy(p => p.CreateAt).FirstOrDefaultAsync().Result.SnapShotId;
        //получаем из EventSourcing записи по данному аукциону, SnapShot
        var financeItem = await _dbContext.EventsLogs.FirstOrDefaultAsync(p =>
            p.SnapShotId == lastSnapShotId &&
            //p.ServiceName == context.ServiceName &&
            p.AuctionId == context.AuctionId
        );
        //если были ставки по данному аукциону - то были списаны деньги
        //тогда financeItem не нулевая, делаем компенсирующую запись
        if (financeItem != null)
        {
            //делаем запись для удаления из финансов, пишем в ES
            JsonSerializerOptions options = new()
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            deleteItem.Value = JsonSerializer.Deserialize<FinanceItem>(context.EventData).Value;
            context.EventData = JsonSerializer.Serialize(deleteItem, deleteItem.GetType(), options);
            await _insertItemToEventSourcing.Processing(context);
            var sendFinanceItem = new ActionMessageList<FinanceItem>
            (
                new List<ActionMessage<FinanceItem>>
                {
                    //объект для удаления платежа а FinanceService
                    new ActionMessage<FinanceItem>
                    (
                        deleteItem,
                        context.OperationType,
                        context.CorrelationId
                    ),
                    //объект для обновления баланса а FinanceService
                    new ActionMessage<FinanceItem>
                    (
                        await GetBalance(context),
                        OperationType.Update,
                        context.CorrelationId
                    )
                },
                context.CallBackType
            );
            await _publishEndpoint.Publish(sendFinanceItem);
        }
        else
        {
            //нет платежей - возвращаемся в state machine
            await _publishEndpoint.Publish(new AuctionDeletedFinance(context.CorrelationId));
        }
        //посылаем в сервис BiddingService для обновления в БД сервиса
        await transaction.CommitAsync();
    }

    private async Task<FinanceItem> GetBalance(ESContract context)
    {
        //получаем записи по пользователю
        var items = await _dbContext.EventsLogs.Where(p =>
            //p.ServiceName == context.ServiceName &&
            p.UserLogin == context.UserLogin)
            .ToListAsync();
        var financeItems = new List<FinanceItem>();
        foreach (var item in items)
        {
            financeItems.Add(JsonSerializer.Deserialize<FinanceItem>(item.EventData));
        }
        //получаем сумму средита
        int? credit = financeItems.Where(p => p.Status == FinanceRecordStatus.Приход).Sum(p => p.Value);
        //получаем сумму дебита
        int? debit = financeItems.Where(p => p.Status == FinanceRecordStatus.Расход).Sum(p => p.Value);
        //баланс
        var balance = (credit ?? 0) - (debit ?? 0);

        //делаем объект на обновление баланса
        var balanceItem = JsonSerializer.Deserialize<FinanceItem>(context.EventData);
        balanceItem.Id = Guid.NewGuid();
        balanceItem.Value = balance;
        balanceItem.Status = FinanceRecordStatus.Баланс;
        balanceItem.UserLogin = context.UserLogin;
        balanceItem.ActionDate = DateTime.UtcNow;

        return balanceItem;
    }
}
