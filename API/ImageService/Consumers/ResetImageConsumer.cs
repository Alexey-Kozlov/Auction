using System.Reflection;
using Common.Contracts.Image;
using Common.Contracts.Processing;
using ImageService.Services;
using MassTransit;

namespace ImageService.Consumers;

public class ResetImageConsumer : IConsumer<ImageReset>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;
    private readonly ImageProceduresService _imageProceduresService;

    public ResetImageConsumer(IPublishEndpoint publishEndpoint,
        IConfiguration configuration, ImageProceduresService imageProceduresService)
    {
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
        _imageProceduresService = imageProceduresService;
    }
    public async Task Consume(ConsumeContext<ImageReset> context)
    {
        try
        {
            var correlationId = context.Message.CorrelationId;
            await _imageProceduresService.ResetItems(correlationId);
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
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "ImageService_ResetImage");
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
