using System.Reflection;
using BiddingService.Services;
using Common.Contracts.Bid;
using Common.Contracts.Processing;
using Common.Utils.Logging;
using MassTransit;

namespace BiddingService.Consumers;

public class CommitBidConsumer : IConsumer<BidCommit>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;
    private readonly BidProceduresService _bidProceduresService;

    public CommitBidConsumer(IPublishEndpoint publishEndpoint,
        IConfiguration configuration, BidProceduresService bidProceduresService)
    {
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
        _bidProceduresService = bidProceduresService;
    }
    public async Task Consume(ConsumeContext<BidCommit> context)
    {
        var correlationId = context.Message.CorrelationId;
        try
        {
            await _bidProceduresService.CommitItems(correlationId, context.Message.Commited);
            var messageObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            messageObject.GetType().GetProperty("CorrelationId").SetValue(messageObject, context.Message.CorrelationId);
            messageObject.GetType().GetProperty("ErrorMessage").SetValue(messageObject, context.Message.ErrorMessage);
            messageObject.GetType().GetProperty("ErrorExceptionStack").SetValue(messageObject, context.Message.ErrorExceptionStack);
            messageObject.GetType().GetProperty("ErrorExceptionInputData").SetValue(messageObject, context.Message.ErrorExceptionInputData);
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, context.Message.ErrorServiceName);
            messageObject.GetType().GetProperty("UserLogin").SetValue(messageObject, context.Message.UserLogin);
            await _publishEndpoint.Publish(messageObject);
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
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "BidService_Commit");
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
