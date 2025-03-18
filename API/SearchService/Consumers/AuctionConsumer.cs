using System.Reflection;
using System.Text.Json;
using AutoMapper;
using Common.Contracts.Auction;
using Common.Contracts.Processing;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;

namespace SearchService.Consumers;

public class AuctionConsumer : IConsumer<DataForProcessingServicesList<AuctionItem>>
{
    private readonly SearchDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public AuctionConsumer(SearchDbContext dbContext, IMapper mapper,
        IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
        _mapper = mapper;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<AuctionItem>> context)
    {
        var correlationId = context.Message.CorrelationId;
        try
        {
            foreach (var auctionItem in context.Message.DataObjects)
            {
                var typedItem = JsonSerializer.Deserialize<AuctionItem>(auctionItem.Data);
                switch (auctionItem.CRUD)
                {
                    case CRUD.Delete:
                        var item = await _dbContext.AuctionItems.FirstOrDefaultAsync(p => p.AuctionId == typedItem.AuctionId);
                        if (item == null)
                        {
                            throw new Exception($"Запись для удаления не найдена");
                        }
                        _dbContext.AuctionItems.Remove(item);
                        break;
                    case CRUD.Create:
                        await _dbContext.AuctionItems.AddAsync(typedItem);
                        break;
                    case CRUD.Update:
                        var item2 = await _dbContext.AuctionItems.FirstOrDefaultAsync(p => p.AuctionId == typedItem.AuctionId);
                        if (item2 == null)
                        {
                            throw new Exception($"Запись для обновления не найдена");
                        }
                        _mapper.Map(typedItem, item2);
                        _dbContext.AuctionItems.Update(item2);
                        break;
                }
            }
            await _dbContext.SaveChangesAsync();
            var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
            await _publishEndpoint.Publish(sendObject);
        }
        catch (Exception e)
        {
            //ошибки, в т.ч. штатные
            var messageObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            messageObject.GetType().GetProperty("CorrelationId").SetValue(messageObject, context.Message.CorrelationId);
            messageObject.GetType().GetProperty("Message").SetValue(messageObject, e.Message);
            messageObject.GetType().GetProperty("ExceptionMessage").SetValue(messageObject, e.StackTrace);
            messageObject.GetType().GetProperty("ServiceName").SetValue(messageObject, "SearchService");
            messageObject.GetType().GetProperty("UserLogin").SetValue(messageObject, "");
            messageObject.GetType().GetProperty("AuctionId").SetValue(messageObject, null);
            messageObject.GetType().GetProperty("IsError").SetValue(messageObject, true);
            var faultType = typeof(FaultMessage<>);
            var typeParams = new Type[] { messageObject.GetType() };
            var faultObjectType = faultType.MakeGenericType(typeParams);

            var faultObject = Activator.CreateInstance(faultObjectType,
                new object[] { "", messageObject });

            await _publishEndpoint.Publish(faultObject);
        }
    }
}
