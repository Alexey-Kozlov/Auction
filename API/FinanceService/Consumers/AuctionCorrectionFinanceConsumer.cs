using Common.Contracts;
using FinanceService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Consumers;

public class AuctionCorrectionFinanceConsumer : IConsumer<FinanceCorrectionStart>
{
    private readonly FinanceDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionCorrectionFinanceConsumer(FinanceDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<FinanceCorrectionStart> context)
    {
        using var transaction = _dbContext.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
        foreach (var item in context.Message.FinanceActionsList)
        {
            var finItem = await _dbContext.FinanceItems.Where(p => p.Id == item.Item1.Id).FirstOrDefaultAsync();
            switch (item.Item2)
            {
                case OperationType.Delete:
                    _dbContext.FinanceItems.Remove(finItem);
                    break;
                case OperationType.Update:
                    finItem.Value = item.Item1.Value;
                    break;
            }
        }
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        //await _publishEndpoint.Publish(new FinanceCorrectionEnd(true));
    }
}
