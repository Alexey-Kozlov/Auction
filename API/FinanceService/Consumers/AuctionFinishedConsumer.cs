using Contracts;
using FinanceService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
{
    private readonly FinanceDbContext _dbContext;

    public AuctionFinishedConsumer(FinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task Consume(ConsumeContext<AuctionFinished> context)
    {
        using var transaction = _dbContext.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
        //подтверждаем у победителя запись дебита
        var winnerItem = await _dbContext.BalanceItems.FirstOrDefaultAsync(p => p.AuctionId == context.Message.AuctionId &&
            p.UserLogin == context.Message.Winner && p.Status == RecordStatus.Заявка);
        winnerItem.Status = RecordStatus.Подтверждено;
        //удаляем у всех записи откатов дебита, если есть
        var rollbackItems = await _dbContext.BalanceItems.Where(p => p.AuctionId == context.Message.AuctionId &&
            p.Status == RecordStatus.Откат).ToListAsync();
        if (rollbackItems != null && rollbackItems.Count > 0)
        {
            _dbContext.RemoveRange(rollbackItems);
        }

        //удаляем все резервирования денег по данному аукциону, кроме победителя
        var debitItems = await _dbContext.BalanceItems.Where(p => p.AuctionId == context.Message.AuctionId &&
            p.UserLogin != context.Message.Winner).ToListAsync();
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
        Console.WriteLine($"{DateTime.Now}  Получение сообщения - аукцион завершен - " + context.Message.AuctionId);
    }
}
