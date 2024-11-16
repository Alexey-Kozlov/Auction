using System.Reflection;
using System.Text.Json;
using AutoMapper;
using Common.Contracts.Auction;
using Common.Contracts.Processing;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;

namespace SearchService.Consumers;

public class SearchConsumer : IConsumer<DataForProcessingServicesList<AuctionItem>>
{
    private readonly IMapper _mapper;
    private readonly SearchDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public SearchConsumer(IMapper mapper, SearchDbContext dbContext,
        IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _mapper = mapper;
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<AuctionItem>> context)
    {
        var correlationId = context.Message.CorrelationId;
        foreach (var auctionItem in context.Message.DataObjects)
        {
            var typedItem = JsonSerializer.Deserialize<AuctionItem>(auctionItem.Data);
            switch (auctionItem.CRUD)
            {
                case CRUD.Delete:
                    var item = await _dbContext.AuctionItems.FirstOrDefaultAsync(p => p.AuctionId == typedItem.AuctionId);
                    if (item != null)
                    {
                        throw new Exception($"Запись для удаления не найдена");
                    }
                    _dbContext.AuctionItems.Remove(item);
                    break;
                case CRUD.Create:
                    await _dbContext.AuctionItems.AddAsync(typedItem);
                    break;
            }
        }

        await _dbContext.SaveChangesAsync();
        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
        await _publishEndpoint.Publish(sendObject);
        // foreach (var actionItem in context.Message.ActionItemsList)
        // {
        //     switch (actionItem.OperationType)
        //     {
        //         case OperationType.Update:
        //             var updateItem = await _dbContext.AuctionItems.FirstOrDefaultAsync(p => p.AuctionId == actionItem.ActionItem.AuctionId);
        //             _mapper.Map(actionItem.ActionItem, updateItem);
        //             break;
        //         case OperationType.Delete:
        //             var deleteItem = await _dbContext.AuctionItems.FirstOrDefaultAsync(p => p.AuctionId == actionItem.ActionItem.AuctionId);
        //             _dbContext.AuctionItems.Remove(deleteItem);
        //             break;
        //         case OperationType.Insert:
        //             _dbContext.AuctionItems.Add(actionItem.ActionItem);
        //             break;
        //         case OperationType.Bid:
        //             var updateItem2 = await _dbContext.AuctionItems.FirstOrDefaultAsync(p => p.AuctionId == actionItem.ActionItem.AuctionId);
        //             updateItem2.CurrentHighBid = actionItem.ActionItem.CurrentHighBid;
        //             break;
        //     }
        // }
    }
}
