using Common.Contracts;
using FinanceService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Consumers;

public class AuctionFinanceConsumer : IConsumer<ActionMessageList<FinanceItem>>
{
    private readonly FinanceDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionFinanceConsumer(FinanceDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<ActionMessageList<FinanceItem>> context)
    {
        var correlationId = context.Message.ActionItemsList[0].CorrelationId;
        foreach (var financeItem in context.Message.ActionItemsList)
        {
            switch (financeItem.OperationType)
            {
                case OperationType.Update:
                    //обновляем баланс
                    var item = await CheckExistItem(financeItem);
                    item.Value = financeItem.ActionItem.Value;
                    await _dbContext.SaveChangesAsync();
                    break;
                //удаляем платеж в случае удаления аукциона
                case OperationType.Delete:
                    var delItem = await CheckExistItem(financeItem);
                    _dbContext.FinanceItems.Remove(delItem);
                    break;
                case OperationType.Insert:
                    _dbContext.FinanceItems.Add(financeItem.ActionItem);
                    await _dbContext.SaveChangesAsync();
                    await _publishEndpoint.Publish(new FinanceCreated(correlationId));
                    break;
            }
        }
        //если в списке объектов есть "удаление" - это удаление аукциона
        if (context.Message.ActionItemsList.Any(p => p.OperationType == OperationType.Delete))
        {
            await _publishEndpoint.Publish(new AuctionDeletedFinance(correlationId));
        }

    }

    private async Task<FinanceItem> CheckExistItem(ActionMessage<FinanceItem> actionItem)
    {
        var item = await _dbContext.FinanceItems.FirstOrDefaultAsync(p => p.AuctionId == actionItem.ActionItem.AuctionId);
        if (item == null)
        {
            Console.WriteLine($"{DateTime.Now} Ошибка обновления записи - запись " + actionItem.ActionItem.AuctionId + " не найдена.");
            throw new Exception($"{DateTime.Now} Ошибка обновления записи - запись " + actionItem.ActionItem.AuctionId + " не найдена.");
        }
        return item;
    }
}
