using Common.Contracts;
using FinanceService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Consumers;

public class AuctionFinishingFinanceConsumer : IConsumer<AuctionFinishingFinance>
{
    private readonly FinanceDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionFinishingFinanceConsumer(FinanceDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<AuctionFinishingFinance> context)
    {
        using var transaction = _dbContext.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
        //если был победитель - подтверждаем его траты
        if (context.Message.ItemSold && !string.IsNullOrEmpty(context.Message.Winner))
        {
            //подтверждаем у победителя запись дебита
            var winnerItem = await _dbContext.BalanceItems.FirstOrDefaultAsync(p => p.AuctionId == context.Message.Id &&
                p.UserLogin == context.Message.Winner && p.Status == RecordStatus.Заявка);
            winnerItem.Status = RecordStatus.Подтверждено;
            //удаляем у всех записи откатов дебита для данного аукциона, если есть
            var rollbackItems = await _dbContext.BalanceItems.Where(p => p.AuctionId == context.Message.Id &&
                p.Status == RecordStatus.Откат).ToListAsync();
            if (rollbackItems != null && rollbackItems.Count > 0)
            {
                _dbContext.RemoveRange(rollbackItems);
            }
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            Console.WriteLine($"{DateTime.Now}  Получение сообщения - аукцион завершен - {context.Message.Id}, " +
            $"победитель - {context.Message.Winner}");
        }

        await _publishEndpoint.Publish(new AuctionFinishedFinance(context.Message.CorrelationId));
        Console.WriteLine($"{DateTime.Now}  Получение сообщения - аукцион завершен - " + context.Message.Id);
    }
}
