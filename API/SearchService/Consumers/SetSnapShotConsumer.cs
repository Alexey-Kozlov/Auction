using System.Reflection;
using System.Text.Json;
using AutoMapper;
using Common.Contracts.Auction;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;

namespace SearchService.Consumers;

public class SetSnapShotConsumer : IConsumer<ESContract>
{
    private readonly IMapper _mapper;
    private readonly SearchDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public SetSnapShotConsumer(IMapper mapper, SearchDbContext context, IPublishEndpoint publishEndpoint,
        IConfiguration configuration)
    {
        _mapper = mapper;
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

            var items = new List<AuctionItem>();
            _mapper.Map(await _context.AuctionItems.ToListAsync(), items);
            foreach (var item in items)
            {
                listItems.DataObjects.Add
                (
                    new DataForProcessingService
                    {
                        DataType = nameof(AuctionItem),
                        Data = JsonSerializer.Serialize(item, item.GetType()),
                        CRUD = CRUD.Create
                    }
                );
            }
            var sendObject = new DataForProcessingServicesList<string>
            {
                CallBackType = context.Message.CallBackType,
                DataObjects = listItems.DataObjects,
                CorrelationId = context.Message.CorrelationId,
                Props = context.Message.EventData
            };

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
            messageObject.GetType().GetProperty("ServiceName").SetValue(messageObject, "SearchService_SetSnapShot");
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
