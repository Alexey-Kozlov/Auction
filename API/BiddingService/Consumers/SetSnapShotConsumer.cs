using System.Reflection;
using System.Text.Json;
using BiddingService.Data;
using Common.Contracts.Bid;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using Common.Utils.Logging;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BiddingService.Consumers;

public class SetSnapShotConsumer : IConsumer<ESContract>
{
    private readonly BidDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public SetSnapShotConsumer(BidDbContext context, IPublishEndpoint publishEndpoint,
        IConfiguration configuration)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<ESContract> context)
    {
        try
        {
            var listItems = new DataForProcessingServicesList
            {
                DataObjects = new List<DataForProcessingService>()
            };

            //получаем записи по таблице Bids
            foreach (var item in await _context.Bids.ToListAsync())
            {
                var rez = JsonSerializer.Serialize(item, item.GetType());
                listItems.DataObjects.Add
                (
                    new DataForProcessingService
                    {
                        DataType = nameof(BidItem),
                        Data = rez,
                        CRUD = CRUD.Create
                    }
                );
            }
            var sendObject = new DataForProcessingServicesList<string>
            {
                CallBackType = context.Message.CallBackType,
                DataObjects = listItems.DataObjects,
                CorrelationId = context.Message.CorrelationId,
                Props = context.Message.EventData,
            };

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
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "BiddingService_SetSnapShot");
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

