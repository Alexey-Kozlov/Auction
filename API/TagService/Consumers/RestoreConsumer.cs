using System.Reflection;
using System.Text.Json;
using Common.Contracts.Processing;
using Common.Contracts.Tag;
using Common.Utils.Logging;
using MassTransit;
using TagService.Data;

namespace TagService.Consumers;

public class RestoreConsumer : IConsumer<DataForProcessingServicesList<TagItem>>
{
    private readonly TagDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public RestoreConsumer(TagDbContext dbContext, IPublishEndpoint publishEndpoint,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<TagItem>> context)
    {
        try
        {
            var correlationId = context.Message.CorrelationId;
            foreach (var item in context.Message.DataObjects)
            {
                var typedItem = JsonSerializer.Deserialize<TagItem>(item.Data);
                typedItem.Commited = false;
                typedItem.CorrelationId = correlationId;
                typedItem.ItemId = typedItem.ItemId;
                await _dbContext.TagItems.AddAsync(typedItem);
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
            messageObject.GetType().GetProperty("ErrorMessage").SetValue(messageObject, GetErrorMessage.GetInnerException(e).Message);
            messageObject.GetType().GetProperty("ErrorExceptionMessage").SetValue(messageObject, e.StackTrace);
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "TagService_Restore");
            messageObject.GetType().GetProperty("UserLogin").SetValue(messageObject, "");
            messageObject.GetType().GetProperty("ItemId").SetValue(messageObject, null);
            messageObject.GetType().GetProperty("IsError").SetValue(messageObject, true);
            var faultType = typeof(FaultMessage<>);
            var typeParams = new Type[] { messageObject.GetType() };
            var faultObjectType = faultType.MakeGenericType(typeParams);
            var faultObject = Activator.CreateInstance(faultObjectType, new object[] { messageObject });

            await _publishEndpoint.Publish(faultObject.GetType().GetMethod("CastItem").Invoke(faultObject, null));
        }
    }

}
