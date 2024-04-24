using Contracts;
using FinanceService.Data;
using FinanceService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Consumers;

public class FinanceDebitAddConsumer : IConsumer<RequestFinanceDebitAdd>
{
    private readonly FinanceDbContext _context;

    public FinanceDebitAddConsumer(FinanceDbContext financeDbContext)
    {
        _context = financeDbContext;
    }
    public async Task Consume(ConsumeContext<RequestFinanceDebitAdd> context)
    {
        using var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
        var balanceItem = await _context.BalanceItems.Where(p => p.UserLogin == context.Message.UserLogin)
            .OrderByDescending(p => p.ActionDate).FirstOrDefaultAsync();
        var debit = new BalanceItem
        {
            ActionDate = DateTime.UtcNow,
            AuctionId = context.Message.AuctionId,
            Balance = (balanceItem?.Balance ?? 0) - context.Message.Debit,
            Credit = 0,
            Debit = context.Message.Debit,
            Reserved = true,
            UserLogin = context.Message.UserLogin
        };
        await _context.BalanceItems.AddAsync(debit);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        await context.Publish(new FinanceGranted(context.Message.CorrelationId));
        Console.WriteLine("--> Получение сообщения - новый дебит, - " +
                 context.Message.Debit + ", " + context.Message.UserLogin);

    }
}
