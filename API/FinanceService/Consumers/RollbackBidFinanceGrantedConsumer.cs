using Contracts;
using FinanceService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Consumers;

public class RollbackBidFinanceGrantedConsumer : IConsumer<RollbackBidFinanceGranted>
{
    private readonly FinanceDbContext _context;

    public RollbackBidFinanceGrantedConsumer(FinanceDbContext financeDbContext)
    {
        _context = financeDbContext;
    }
    public async Task Consume(ConsumeContext<RollbackBidFinanceGranted> context)
    {
        using var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
        var balanceItem = await _context.BalanceItems.Where(p => p.UserLogin == context.Message.Bidder)
            .OrderByDescending(p => p.ActionDate).FirstOrDefaultAsync();
        var rallbackAuctionBid = await _context.BalanceItems.Where(p => p.UserLogin == context.Message.Bidder &&
            p.AuctionId == context.Message.AuctionId && p.Status == RecordStatus.Откат).FirstOrDefaultAsync();
        var newAuctionBid = await _context.BalanceItems.Where(p => p.UserLogin == context.Message.Bidder &&
                    p.AuctionId == context.Message.AuctionId && p.Status == RecordStatus.Заявка).FirstOrDefaultAsync();
        if (rallbackAuctionBid != null)
        {
            //нашли заявку для отката - делаем ее рабочей и удаляем последнюю
            var debitDiff = newAuctionBid.Debit - rallbackAuctionBid.Debit;
            var currentCredit = (balanceItem?.Balance ?? 0) + debitDiff;
            rallbackAuctionBid.Status = RecordStatus.Заявка;
            balanceItem.Debit = currentCredit;
        }
        else
        {
            var currentCredit = (balanceItem?.Balance ?? 0) + newAuctionBid.Debit;
            balanceItem.Debit = currentCredit;
        }
        _context.BalanceItems.Remove(newAuctionBid);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        Console.WriteLine($"{DateTime.Now} Получение сообщения - отмена платежа по заявке, - " +
                 context.Message.Amount + ", " + context.Message.Bidder);

    }
}
