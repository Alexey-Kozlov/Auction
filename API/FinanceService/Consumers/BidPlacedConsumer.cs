using Contracts;
using FinanceService.Data;
using FinanceService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    private readonly FinanceDbContext _context;

    public BidPlacedConsumer(FinanceDbContext financeDbContext)
    {
        _context = financeDbContext;
    }
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        using var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
        var balanceItem = await _context.BalanceItems.Where(p => p.UserLogin == context.Message.Bidder)
            .OrderByDescending(p => p.ActionDate).FirstOrDefaultAsync();
        var debit = new BalanceItem
        {
            ActionDate = DateTime.UtcNow,
            AuctionId = context.Message.AuctionId,
            Balance = (balanceItem?.Balance ?? 0) - context.Message.Amount,
            Credit = 0,
            Debit = context.Message.Amount,
            Reserved = false,
            UserLogin = context.Message.Bidder
        };
        await _context.BalanceItems.AddAsync(debit);
        await _context.SaveChangesAsync();
        Console.WriteLine("--> Получение сообщения - размещена заявка - " + context.Message.Id + ", "
                 + context.Message.Amount + ", " + context.Message.Bidder);

    }
}
