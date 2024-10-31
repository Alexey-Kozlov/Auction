using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Contracts;
using EventSourcingService.Data;
using EventSourcingService.Entities;
using EventSourcingService.Services.FinanceProcessing;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Services.CreateEventSourcingProcessing;

public class FinanceProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public FinanceProcessing(IPublishEndpoint publishEndpoint,
        EventSourcingDbContext dbContext, IConfiguration configuration)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _configuration = configuration;
    }
    public async Task Processing(ConsumeContext<ESContract> context)
    {
        switch (context.Message.EntityType)
        {
            //ProcessingService -> Activities -> AuctionDelete -> FinanceActivity                         
            case nameof(AuctionDeletingFinance):
                //вызываем сервис по корректировке платежей по аукциону
                //await _deleteAuctionFinanceCorrection.Processing(context.Message);
                await _publishEndpoint.Publish(new AuctionDeleteESFinance(context.Message.CorrelationId));
                break;
            //ProcessingService -> Activities -> Finance -> FinanceActivity                         
            case nameof(FinanceItem):
                //вызываем сервис по добавлению денег
                await FinanceCreate(context.Message);
                break;
        }
    }

    private async Task FinanceCreate(ESContract context)
    {
        using var transaction = _dbContext.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
        //получаем записи по пользователю
        var items = await _dbContext.EventsLogs.Where(p =>
            p.ServiceName == context.ServiceName &&
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
        var message = JsonSerializer.Deserialize<FinanceItem>(context.EventData);
        //делаем операцию для добавления денег
        var addFinance = new FinanceCreateMessage(message, OperationType.Insert, context.CorrelationId);
        //делаем операцию для обновления баланса

        JsonSerializerOptions options = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var balanceItem = JsonSerializer.Deserialize<FinanceItem>(context.EventData);
        balanceItem.Id = Guid.NewGuid();
        balanceItem.Value = balance;
        balanceItem.Status = FinanceRecordStatus.Баланс;
        balanceItem.UserLogin = context.UserLogin;
        balanceItem.ActionDate = DateTime.UtcNow;
        var updateBalance = new FinanceCreateMessage(balanceItem, OperationType.Update, context.CorrelationId);

        //добавляем запись баланса в EventSourcing
        _dbContext.EventsLogs.Add(new EventsLog
        {
            CorrelationId = context.CorrelationId,
            CreateAt = DateTime.UtcNow,
            Commited = false,
            EntityType = context.EntityType,
            ServiceName = context.ServiceName,
            EventData = JsonDocument.Parse(JsonSerializer.Serialize(balanceItem, balanceItem.GetType(), options)),
            LogicVersion = int.Parse(_configuration["LogicVersion"]),
            UserLogin = context.UserLogin,
            AuctionId = context.AuctionId,
            OperationType = OperationType.Update
        });
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        var financeToDo = new List<FinanceCreateMessage>
        {
            addFinance,
            updateBalance
        };
        await _publishEndpoint.Publish(new FinanceAddCredit(financeToDo));
    }
}