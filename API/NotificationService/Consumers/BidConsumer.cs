using System.Reflection;
using System.Text.Json;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using Common.Utils.Logging;
using MassTransit;
using NotificationService.Data;

namespace NotificationService.Consumers;

public class BidConsumer : IConsumer<DataForProcessingServicesList<NotifyItem>>
{
    private readonly NotificationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public BidConsumer(NotificationDbContext dbContext, IPublishEndpoint publishEndpoint,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<NotifyItem>> context)
    {
        var correlationId = context.Message.CorrelationId;
        try
        {
            foreach (var item in context.Message.DataObjects)
            {
                var typedItem = JsonSerializer.Deserialize<NotifyItem>(item.Data);
                if (item.CRUD == CRUD.Create)
                {
                    await _dbContext.NotifyItems.AddAsync(new NotifyItem
                    {
                        Commited = false,
                        ItemId = typedItem.ItemId,
                        UserLogin = typedItem.UserLogin,
                        CorrelationId = correlationId
                    });
                    await _dbContext.SaveChangesAsync();
                }
            }

            var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
            await _publishEndpoint.Publish(sendObject);
        }
        catch (Exception e)
        {
            //ошибки прочие
            var messageObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            messageObject.GetType().GetProperty("CorrelationId").SetValue(messageObject, context.Message.CorrelationId);
            messageObject.GetType().GetProperty("ErrorMessage").SetValue(messageObject, GetErrorMessage.GetInnerException(e).Message);
            messageObject.GetType().GetProperty("ErrorExceptionMessage").SetValue(messageObject, e.StackTrace);
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "NotificationService_Bid");
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
