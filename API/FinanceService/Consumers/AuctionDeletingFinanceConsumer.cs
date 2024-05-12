using Contracts;
using FinanceService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Consumers;

public class AuctionDeletingFinanceConsumer : IConsumer<AuctionDeletingFinance>
{
    private readonly FinanceDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionDeletingFinanceConsumer(FinanceDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<AuctionDeletingFinance> context)
    {
        using var transaction = _dbContext.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
        //удаляем у всех записи откатов дебита, если есть
        var rollbackItems = await _dbContext.BalanceItems.Where(p => p.AuctionId == context.Message.Id &&
            p.Status == RecordStatus.Откат).ToListAsync();
        if (rollbackItems != null && rollbackItems.Count > 0)
        {
            _dbContext.RemoveRange(rollbackItems);
        }

        //удаляем все резервирования денег по данному аукциону
        var debitItems = await _dbContext.BalanceItems.Where(p => p.AuctionId == context.Message.Id).ToListAsync();
        //правим финансы для каждого пользователя, участвующего в аукционе (кроме победителя)
        //возвращаем деньги за проигранный аукцион
        var balance = 0;
        foreach (var debitItem in debitItems)
        {
            var balanceItem = await _dbContext.BalanceItems.Where(p => p.UserLogin == debitItem.UserLogin)
                .OrderByDescending(p => p.ActionDate).FirstOrDefaultAsync();
            balance = balanceItem.Balance + debitItem.Debit;
            _dbContext.Remove(debitItem);
            balanceItem = await _dbContext.BalanceItems.Where(p => p.UserLogin == debitItem.UserLogin)
                .OrderByDescending(p => p.ActionDate).FirstOrDefaultAsync();
            balanceItem.Balance = balance;
        }
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        await _publishEndpoint.Publish(new AuctionDeletedFinance(context.Message.CorrelationId));
        Console.WriteLine($"{DateTime.Now}  Получение сообщения - аукцион удален - " + context.Message.Id);
    }
}
