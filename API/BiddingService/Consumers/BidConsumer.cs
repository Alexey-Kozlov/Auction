using System.Reflection;
using System.Text.Json;
using BiddingService.Data;
using Common.Contracts.Bid;
using Common.Contracts.Processing;
using MassTransit;
using Microsoft.EntityFrameworkCore;


namespace BiddingService.Consumers;

public class BidConsumer : IConsumer<DataForProcessingServicesList<BidItem>>
{
    private readonly BidDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public BidConsumer(BidDbContext dbContext, IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<BidItem>> context)
    {
        var correlationId = context.Message.CorrelationId;
        try
        {
            if (context.Message.DataObjects != null)
            {
                foreach (var item in context.Message.DataObjects)
                {
                    var typedItem = JsonSerializer.Deserialize<BidItem>(item.Data);
                    typedItem.CorrelationId = correlationId;
                    switch (item.CRUD)
                    {
                        case CRUD.Create:
                            typedItem.Id = Guid.NewGuid();
                            typedItem.Commited = false;
                            await _dbContext.Bids.AddAsync(typedItem);
                            break;
                        case CRUD.Delete:
                            //удаляем запись
                            var delItem = await _dbContext.Bids.FirstOrDefaultAsync(p =>
                                p.BidId == typedItem.BidId && p.Commited);
                            if (delItem == null)
                            {
                                throw new Exception($"Запись для удаления не найдена");
                            }
                            delItem.CorrelationId = correlationId;
                            _dbContext.Bids.Update(delItem);
                            break;
                        default:
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
            messageObject.GetType().GetProperty("ErrorMessage").SetValue(messageObject, e.Message);
            messageObject.GetType().GetProperty("ErrorExceptionMessage").SetValue(messageObject, e.StackTrace);
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "BidService");
            messageObject.GetType().GetProperty("UserLogin").SetValue(messageObject, "");
            messageObject.GetType().GetProperty("AuctionId").SetValue(messageObject, null);
            messageObject.GetType().GetProperty("IsError").SetValue(messageObject, true);
            var faultType = typeof(FaultMessage<>);
            var typeParams = new Type[] { messageObject.GetType() };
            var faultObjectType = faultType.MakeGenericType(typeParams);

            var faultObject = Activator.CreateInstance(faultObjectType, new object[] { messageObject });

            await _publishEndpoint.Publish(faultObject.GetType().GetMethod("CastItem").Invoke(faultObject, null));
        }
    }
}
