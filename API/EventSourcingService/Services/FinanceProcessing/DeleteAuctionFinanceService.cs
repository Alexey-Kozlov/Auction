using System.Collections;
using System.Text.Json;
using Common.Contracts;
using EventSourcingService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Services.FinanceProcessing;

public class DeleteAuctionFinanceService
{
    private readonly EventSourcingDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IPublishEndpoint _publishEndpoint;

    public DeleteAuctionFinanceService(EventSourcingDbContext dbContext, IConfiguration configuration,
        IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Processing(ESContract context)
    {
        using var transaction = _dbContext.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
        var lastSnapShotId = _dbContext.EventsLogs.Where(p => p.SnapShotId != null)
            .OrderBy(p => p.CreateAt).FirstOrDefaultAsync().Result.SnapShotId;
        //получаем из EventSourcing записи по данному аукциону, SnapShot
        var items = await _dbContext.EventsLogs.Where(p =>
            p.SnapShotId == lastSnapShotId &&
            p.ServiceName == _configuration["ServicesName:FinanceService"] &&
            p.AuctionId == context.AuctionId
        ).ToListAsync();
        var financeItems = new List<FinanceItem>();
        foreach (var item in items)
        {
            financeItems.Add(JsonSerializer.Deserialize<FinanceItem>(item.EventData));
        }
        //создаем коллекцию будущих изменений в сервисе Finance
        var financeToDo = new List<(FinanceItem, OperationType)>();
        var delItem = financeItems.Where(p => p.Status == FinanceRecordStatus.Приход).FirstOrDefault();

        //удаляем запись отката дебита, если есть
        if (delItem != null)
        {
            (FinanceItem, OperationType) delFinance = (delItem, OperationType.Delete);
            financeToDo.Add(delFinance);
        }


        //var balance = 0;

        //удаляем все резервирования денег по данному аукциону
        //правим финансы у пользователя - у которого максимальная ставка
        //возвращаем деньги за отмененный аукцион

        //значение ставки для отмены
        // var balanceItem = financeItems.Where(p => p.Status ==  FinanceRecordStatus.Баланс).FirstOrDefault();
        // balance = balanceItem.Balance;
        // //получаем запись о последнем поступлении денег на счет пользователя
        // financeItems.Clear();
        // items = await _dbContext.EventsLogs.Where(p =>
        //     p.SnapShotId == lastSnapShotId &&
        //     p.ServiceName == _configuration["ServicesName:FinanceService"] &&
        //     p.UserLogin == balanceItem.UserLogin).ToListAsync();
        // foreach (var item in items)
        // {
        //     financeItems.Add(JsonSerializer.Deserialize<FinanceItem>(item.EventData));
        // }
        // var lastFinance = financeItems.Where(p => p.Status == RecordStatus.Подтверждено)
        //     .OrderByDescending(p => p.ActionDate).FirstOrDefault();
        // lastFinance.Balance += balance;

        // (FinanceItem, FinanceAction) updateFinance = (lastFinance, FinanceAction.Update);
        // financeToDo.Add(updateFinance);
        await transaction.CommitAsync();

        await _publishEndpoint.Publish(new FinanceCorrectionStart(financeToDo));
    }
}