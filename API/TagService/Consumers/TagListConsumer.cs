using System.Reflection;
using System.Text.Json;
using Common.Contracts.Processing;
using Common.Contracts.Tag;
using Common.Utils.Logging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using TagService.Data;

namespace TagService.Consumers;

public class TagListConsumer : IConsumer<TagListRequest>
{
    private readonly TagDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public TagListConsumer(IPublishEndpoint publishEndpoint, TagDbContext dbContext,
        IConfiguration configuration)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<TagListRequest> context)
    {
        try
        {
            var itemList = await _dbContext.TagLists.ToListAsync();
            var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
            sendObject.GetType().GetProperty("Data").SetValue(sendObject, JsonSerializer.Serialize(itemList,
            new JsonSerializerOptions { PropertyNamingPolicy = new LowercaseNamingPolicy() }));
            await _publishEndpoint.Publish(sendObject);
        }
        catch (Exception e)
        {
            //ошибки прочие
            var messageObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            messageObject.GetType().GetProperty("CorrelationId").SetValue(messageObject, context.Message.CorrelationId);
            messageObject.GetType().GetProperty("ErrorMessage").SetValue(messageObject, GetErrorMessage.GetMessage(e));
            messageObject.GetType().GetProperty("ErrorExceptionStack").SetValue(messageObject, e.StackTrace);
            messageObject.GetType().GetProperty("ErrorExceptionInputData").SetValue(messageObject, GetErrorMessage.GetExceptionStringData(context.Message));
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "TagService_CreateTagConsumer");


            var faultType = typeof(FaultMessage<>);
            var typeParams = new Type[] { messageObject.GetType() };
            var faultObjectType = faultType.MakeGenericType(typeParams);

            var faultObject = Activator.CreateInstance(faultObjectType, new object[] { messageObject });

            await _publishEndpoint.Publish(faultObject.GetType().GetMethod("CastItem").Invoke(faultObject, null));
        }
    }
}

public class LowercaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) => name.ToLower();
}
