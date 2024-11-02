using System.Reflection;
using BiddingService.Data;
using Common.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;


namespace BiddingService.Consumers;

public class AuctionBidConsumer : IConsumer<ActionMessageList<AuctionBidItem>>
{
    private readonly BidDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public AuctionBidConsumer(BidDbContext dbContext, IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<ActionMessageList<AuctionBidItem>> context)
    {
        var correlationId = context.Message.ActionItemsList[0].CorrelationId;
        foreach (var actionItem in context.Message.ActionItemsList)
        {
            switch (actionItem.OperationType)
            {
                case OperationType.Update:
                    var item = await CheckExistItem(actionItem);
                    item.AuctionEnd = actionItem.ActionItem.AuctionEnd;
                    break;
                case OperationType.Delete:
                    var delItem = await CheckExistItem(actionItem);
                    _dbContext.Auctions.Remove(delItem);
                    break;
                case OperationType.Insert:
                    _dbContext.Auctions.Add(actionItem.ActionItem);
                    break;
            }
        }
        await _dbContext.SaveChangesAsync();
        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
        await _publishEndpoint.Publish(sendObject);
    }

    private async Task<AuctionBidItem> CheckExistItem(ActionMessage<AuctionBidItem> actionItem)
    {
        var item = await _dbContext.Auctions.FirstOrDefaultAsync(p => p.AuctionId == actionItem.ActionItem.AuctionId);
        if (item == null)
        {
            Console.WriteLine($"{DateTime.Now} Ошибка обновления записи - запись " + actionItem.ActionItem.AuctionId + " не найдена.");
            throw new Exception($"{DateTime.Now} Ошибка обновления записи - запись " + actionItem.ActionItem.AuctionId + " не найдена.");
        }
        return item;
    }
}
