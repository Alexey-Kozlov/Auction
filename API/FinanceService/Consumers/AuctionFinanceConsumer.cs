using System.Reflection;
using Common.Contracts.EventSourcing;
using Common.Contracts.Finance;
using FinanceService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Consumers;

public class AuctionFinanceConsumer : IConsumer<ActionMessageList<FinanceItem>>
{
    private readonly FinanceDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public AuctionFinanceConsumer(FinanceDbContext dbContext, IPublishEndpoint publishEndpoint,
         IConfiguration configuration)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<ActionMessageList<FinanceItem>> context)
    {
        var correlationId = context.Message.ActionItemsList[0].CorrelationId;
        foreach (var financeItem in context.Message.ActionItemsList)
        {
            // switch (financeItem.OperationType)
            // {
            //     case OperationType.Update:
            //         //обновляем баланс
            //         var item = await CheckExistItem(financeItem, true);
            //         item.Value = financeItem.ActionItem.Value;
            //         break;
            //     //удаляем платеж в случае удаления аукциона
            //     case OperationType.Delete:
            //         var delItem = await CheckExistItem(financeItem);
            //         _dbContext.FinanceItems.Remove(delItem);
            //         break;
            //     case OperationType.Insert:
            //         _dbContext.FinanceItems.Add(financeItem.ActionItem);
            //         break;
            // }
        }
        await _dbContext.SaveChangesAsync();
        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
        await _publishEndpoint.Publish(sendObject);

    }

    private async Task<FinanceItem> CheckExistItem(ActionMessage<FinanceItem> actionItem, bool isBalance = false)
    {
        //если нет записи баланса - создаем
        if (isBalance)
        {
            var balanceItem = await _dbContext.FinanceItems.FirstOrDefaultAsync(p =>
            p.UserLogin == actionItem.ActionItem.UserLogin &&
            p.Status == FinanceRecordStatus.Баланс);
            if (balanceItem == null)
            {
                await _dbContext.FinanceItems.AddAsync(actionItem.ActionItem);
                await _dbContext.SaveChangesAsync();
            }
        }
        var item = await _dbContext.FinanceItems.FirstOrDefaultAsync(p =>
            p.UserLogin == actionItem.ActionItem.UserLogin &&
            ((!isBalance && p.Status == FinanceRecordStatus.Расход &&
                p.AuctionId == actionItem.ActionItem.AuctionId) ||
            (isBalance && p.Status == FinanceRecordStatus.Баланс)));
        if (item == null)
        {
            Console.WriteLine($"{DateTime.Now} Ошибка обновления записи - запись " + actionItem.ActionItem.AuctionId + " не найдена.");
            throw new Exception($"{DateTime.Now} Ошибка обновления записи - запись " + actionItem.ActionItem.AuctionId + " не найдена.");
        }
        return item;
    }
}
