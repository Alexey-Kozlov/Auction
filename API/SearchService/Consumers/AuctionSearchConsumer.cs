using System.Reflection;
using AutoMapper;
using Common.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;

namespace SearchService.Consumers;

public class AuctionSearchConsumer : IConsumer<ActionMessageList<AuctionItem>>
{
    private readonly IMapper _mapper;
    private readonly SearchDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public AuctionSearchConsumer(IMapper mapper, SearchDbContext dbContext,
        IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _mapper = mapper;
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<ActionMessageList<AuctionItem>> context)
    {
        var correlationId = context.Message.ActionItemsList[0].CorrelationId;
        foreach (var actionItem in context.Message.ActionItemsList)
        {
            switch (actionItem.OperationType)
            {
                case OperationType.Update:
                    _mapper.Map(actionItem.ActionItem, await CheckExistItem(actionItem));
                    break;
                case OperationType.Delete:
                    var delItem = await CheckExistItem(actionItem);
                    _dbContext.AuctionItems.Remove(delItem);
                    break;
                case OperationType.Insert:
                    _dbContext.AuctionItems.Add(actionItem.ActionItem);
                    break;
            }
        }
        await _dbContext.SaveChangesAsync();
        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
        await _publishEndpoint.Publish(sendObject);
    }
    private async Task<AuctionItem> CheckExistItem(ActionMessage<AuctionItem> actionItem)
    {
        var item = await _dbContext.AuctionItems.FirstOrDefaultAsync(p => p.AuctionId == actionItem.ActionItem.AuctionId);
        if (item == null)
        {
            Console.WriteLine($"{DateTime.Now} Ошибка обновления записи - запись " + actionItem.ActionItem.AuctionId + " не найдена.");
            throw new Exception($"{DateTime.Now} Ошибка обновления записи - запись " + actionItem.ActionItem.AuctionId + " не найдена.");
        }
        return item;
    }
}
