using Contracts;
using FinanceService.Data;
using FinanceService.Entities;
using FinanceService.Exceptions;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Consumers;

public class DebitAddConsumer : IConsumer<RequestFinanceDebitAdd>
{
    private readonly FinanceDbContext _context;

    public DebitAddConsumer(FinanceDbContext financeDbContext)
    {
        _context = financeDbContext;
    }
    public async Task Consume(ConsumeContext<RequestFinanceDebitAdd> context)
    {
        using var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
        //проверяем - есть ли записи по данному пользователю, по данному аукциону со статусом откат
        //емли есть - удаляем
        var rallbackAuctionBid = await _context.BalanceItems.Where(p => p.UserLogin == context.Message.UserLogin &&
            p.AuctionId == context.Message.AuctionId && p.Status == RecordStatus.Откат).ToListAsync();
        if (rallbackAuctionBid.Any())
        {
            _context.BalanceItems.RemoveRange(rallbackAuctionBid);
        }

        var balanceItem = await _context.BalanceItems.Where(p => p.UserLogin == context.Message.UserLogin)
            .OrderByDescending(p => p.ActionDate).FirstOrDefaultAsync();
        var lastAuctionBid = await _context.BalanceItems.Where(p => p.UserLogin == context.Message.UserLogin &&
            p.AuctionId == context.Message.AuctionId).OrderByDescending(p => p.ActionDate).FirstOrDefaultAsync();

        if (lastAuctionBid != null)
        {
            //уже была ставка данного пользователя по данному аукциону
            //делаем еще одну, у старой ставим признак возможного отката

            //сначала проверяем на достаточность денег на ставку
            var debitDiff = context.Message.Debit - lastAuctionBid.Debit;
            var currentCredit = (balanceItem?.Balance ?? 0) - debitDiff;
            if (currentCredit < 0)
            {
                //недостаточно денег для новой ставки
                throw new UnsufficientFinanceException(context.Message.UserLogin, context.Message.Debit);
            }
            lastAuctionBid.Status = RecordStatus.Откат;
            var debit = new BalanceItem
            {
                ActionDate = DateTime.UtcNow,
                AuctionId = context.Message.AuctionId,
                Balance = currentCredit,
                Credit = 0,
                Debit = context.Message.Debit,
                Status = RecordStatus.Заявка,
                UserLogin = context.Message.UserLogin
            };
            await _context.BalanceItems.AddAsync(debit);
        }
        else
        {
            //первая заявка данного пользователя на данном аукционе
            var currentCredit = (balanceItem?.Balance ?? 0) - context.Message.Debit;
            if (currentCredit < 0)
            {
                //недостаточно денег для новой ставки
                throw new UnsufficientFinanceException(context.Message.UserLogin, context.Message.Debit);
            }
            var _debit = new BalanceItem
            {
                ActionDate = DateTime.UtcNow,
                AuctionId = context.Message.AuctionId,
                Balance = currentCredit,
                Credit = 0,
                Debit = context.Message.Debit,
                Status = RecordStatus.Заявка,
                UserLogin = context.Message.UserLogin
            };
            await _context.BalanceItems.AddAsync(_debit);
        }
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        await context.Publish(new FinanceGranted(context.Message.CorrelationId));
        Console.WriteLine("--> Получение сообщения - новый дебит, - " +
                 context.Message.Debit + ", " + context.Message.UserLogin);

    }
}
