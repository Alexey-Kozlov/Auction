using System.Reflection;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using Common.Utils.Logging;
using EventSourcingService.Services;
using MassTransit;

namespace SearchService.Consumers;

public class StartFinishServiceConsumer : IConsumer<StartFinishService>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;
    private readonly CheckAuctionFinished _checkAuctionFinished;

    public StartFinishServiceConsumer(IPublishEndpoint publishEndpoint, IConfiguration configuration,
        CheckAuctionFinished checkAuctionFinished)
    {
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
        _checkAuctionFinished = checkAuctionFinished;
    }
    public async Task Consume(ConsumeContext<StartFinishService> context)
    {
        try
        {
            //включаем/выключаем сервис проверки завершения аукционов
            if (!_checkAuctionFinished.IsRunning)
            {
                await _checkAuctionFinished.StartAsync(new CancellationToken());
            }
            var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
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
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "EventSourcingService_ControlFinishService");
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
