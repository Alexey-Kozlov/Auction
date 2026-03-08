using System.Reflection;
using Common.Contracts.Processing;
using Common.Contracts.Tag;
using Common.Utils.Logging;
using MassTransit;
using TagService.Services;

namespace TagService.Consumers;

public class CommitTagConsumer : IConsumer<TagCommit>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;
    private readonly TagProceduresService _tagProceduresService;

    public CommitTagConsumer(IPublishEndpoint publishEndpoint,
        IConfiguration configuration, TagProceduresService tagProceduresService)
    {
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
        _tagProceduresService = tagProceduresService;
    }
    public async Task Consume(ConsumeContext<TagCommit> context)
    {
        var correlationId = context.Message.CorrelationId;
        try
        {
            await _tagProceduresService.CommitItems(correlationId, context.Message.Commited);
            var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
            sendObject.GetType().GetProperty("ErrorMessage").SetValue(sendObject, context.Message.ErrorMessage);
            sendObject.GetType().GetProperty("ErrorExceptionMessage").SetValue(sendObject, context.Message.ErrorExceptionMessage);
            sendObject.GetType().GetProperty("ErrorServiceName").SetValue(sendObject, context.Message.ErrorServiceName);
            sendObject.GetType().GetProperty("UserLogin").SetValue(sendObject, context.Message.UserLogin);
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
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "FinanceService_Commit");
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
