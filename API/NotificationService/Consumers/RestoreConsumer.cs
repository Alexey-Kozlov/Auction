using System.Reflection;
using System.Text.Json;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using Common.Utils.Logging;
using MassTransit;
using NotificationService.Data;

namespace NotificationService.Consumers;

public class RestoreConsumer : IConsumer<DataForProcessingServicesList<NotifyItem>>
{
    private readonly NotificationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public RestoreConsumer(NotificationDbContext dbContext, IPublishEndpoint publishEndpoint,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<NotifyItem>> context)
    {
        try
        {
            var correlationId = context.Message.CorrelationId;
            foreach (var item in context.Message.DataObjects)
            {
                var typedItem = JsonSerializer.Deserialize<NotifyItem>(item.Data);
                typedItem.Commited = false;
                typedItem.CorrelationId = correlationId;
                typedItem.ItemId = typedItem.ItemId;
                await _dbContext.NotifyItems.AddAsync(typedItem);
            }
            if (context.Message.DataObjects.Any())
            {
                await _dbContext.SaveChangesAsync();
            }
            if (!string.IsNullOrEmpty(context.Message.CallBackType))
            {
                var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                    _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
                sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
                await _publishEndpoint.Publish(sendObject);
            }
        }
        catch (Exception e)
        {
            //ошибки, в т.ч. штатные
            var messageObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            messageObject.GetType().GetProperty("CorrelationId").SetValue(messageObject, context.Message.CorrelationId);
            messageObject.GetType().GetProperty("ErrorMessage").SetValue(messageObject, GetErrorMessage.GetMessage(e));
            messageObject.GetType().GetProperty("ErrorExceptionStack").SetValue(messageObject, e.StackTrace);
            messageObject.GetType().GetProperty("ErrorExceptionInputData").SetValue(messageObject, GetErrorMessage.GetExceptionStringData(context.Message));
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "NotificationService_Restore");
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
