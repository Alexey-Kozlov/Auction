using System.Reflection;
using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Processing;
using Common.Utils.Logging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;

namespace SearchService.Consumers;

public class AuctionConsumer : IConsumer<DataForProcessingServicesList<AuctionItem>>
{
    private readonly SearchDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public AuctionConsumer(SearchDbContext dbContext,
        IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<AuctionItem>> context)
    {
        var correlationId = context.Message.CorrelationId;
        try
        {
            if (context.Message.DataObjects != null)
            {
                foreach (var auctionItem in context.Message.DataObjects)
                {
                    var typedItem = JsonSerializer.Deserialize<AuctionItem>(auctionItem.Data);
                    typedItem.CorrelationId = correlationId;
                    switch (auctionItem.CRUD)
                    {
                        case CRUD.Delete:
                            var item = await _dbContext.AuctionItems.FirstOrDefaultAsync(p =>
                                p.ItemId == typedItem.ItemId && p.Commited);
                            if (item == null)
                            {
                                throw new Exception($"Запись для удаления не найдена, AuctionId - {typedItem.ItemId}");
                            }
                            item.CorrelationId = correlationId;
                            _dbContext.AuctionItems.Update(item);
                            break;
                        case CRUD.Create:
                            typedItem.Commited = false;
                            await _dbContext.AuctionItems.AddAsync(typedItem);
                            break;
                        case CRUD.Update:
                            var item2 = await _dbContext.AuctionItems.FirstOrDefaultAsync(p =>
                                p.ItemId == typedItem.ItemId && p.Commited);
                            if (item2 == null)
                            {
                                throw new Exception($"Запись для обновления не найдена, AuctionId - {typedItem.ItemId}");
                            }
                            item2.CorrelationId = correlationId;
                            _dbContext.AuctionItems.Update(item2);
                            typedItem.Commited = false;
                            await _dbContext.AuctionItems.AddAsync(typedItem);
                            break;
                    }
                }
                await _dbContext.SaveChangesAsync();
            }
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
            messageObject.GetType().GetProperty("ErrorMessage").SetValue(messageObject, GetErrorMessage.GetInnerException(e).Message);
            messageObject.GetType().GetProperty("ErrorExceptionMessage").SetValue(messageObject, e.StackTrace);
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "SearchService");
            messageObject.GetType().GetProperty("UserLogin").SetValue(messageObject, "");
            messageObject.GetType().GetProperty("ItemId").SetValue(messageObject, null);

            var faultType = typeof(FaultMessage<>);
            var typeParams = new Type[] { messageObject.GetType() };
            var faultObjectType = faultType.MakeGenericType(typeParams);

            var faultObject = Activator.CreateInstance(faultObjectType, new object[] { messageObject });

            await _publishEndpoint.Publish(faultObject.GetType().GetMethod("CastItem").Invoke(faultObject, null));
        }
    }
}
