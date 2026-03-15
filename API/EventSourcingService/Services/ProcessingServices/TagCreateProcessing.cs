using System.Reflection;
using AuctionService.Metrics;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using Common.Utils.Logging;
using EventSourcingService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Services;

public class TagCreateProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly AuctionMetrics _auctionMetrics;

    public TagCreateProcessing(IPublishEndpoint publishEndpoint, EventSourcingDbContext dbContext,
        IConfiguration configuration, AuctionMetrics auctionMetrics)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _configuration = configuration;
        _auctionMetrics = auctionMetrics;
    }

    public async Task ProcessESLog(ConsumeContext<ESContract> context)
    {
        var listItems = new DataForProcessingServicesList
        {
            DataObjects = new List<DataForProcessingService>()
        };
        try
        {
            var result = await _dbContext.tag_create(
                context.Message.CorrelationId,
                context.Message.ItemId.Value,
                context.Message.AuctionId.Value,
                context.Message.EventData,
                context.Message.UserLogin).ToListAsync();
            foreach (var item in result)
            {
                listItems.DataObjects.Add
                (
                    new DataForProcessingService
                    {
                        DataType = item.entitytype,
                        Data = item.eventdata,
                        CRUD = (CRUD)item.crud
                    }
                );
            }
            _auctionMetrics.CreateTag();
            var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
            sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, listItems);
            await _publishEndpoint.Publish(sendObject);
        }
        catch (Npgsql.PostgresException e)
        {
            //ошибка при выполнении транзакции в БД
            Fault<ESLogTagCreated> errorObj = new FaultMessage<ESLogTagCreated>(
                new ESLogTagCreated
                {
                    CorrelationId = context.Message.CorrelationId,
                    ErrorServiceName = "EventSourcingService_CreateTag",
                    ErrorMessage = e.MessageText,
                    ErrorExceptionMessage = e.Message,
                    UserLogin = context.Message.UserLogin,
                    AuctionId = context.Message.AuctionId,
                }
            );
            await _publishEndpoint.Publish(errorObj);
        }
        catch (Exception e)
        {
            //ошибки прочие
            var messageObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            messageObject.GetType().GetProperty("CorrelationId").SetValue(messageObject, context.Message.CorrelationId);
            messageObject.GetType().GetProperty("ErrorMessage").SetValue(messageObject, GetErrorMessage.GetInnerException(e).Message);
            messageObject.GetType().GetProperty("ErrorExceptionMessage").SetValue(messageObject, e.StackTrace);
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "EventSourcingService_CreateTag");
            messageObject.GetType().GetProperty("UserLogin").SetValue(messageObject, context.Message.UserLogin);
            messageObject.GetType().GetProperty("ItemId").SetValue(messageObject, context.Message.AuctionId);


            var faultType = typeof(FaultMessage<>);
            var typeParams = new Type[] { messageObject.GetType() };
            var faultObjectType = faultType.MakeGenericType(typeParams);

            var faultObject = Activator.CreateInstance(faultObjectType, new object[] { messageObject });
            await _publishEndpoint.Publish(faultObject.GetType().GetMethod("CastItem").Invoke(faultObject, null));
        }
    }

}