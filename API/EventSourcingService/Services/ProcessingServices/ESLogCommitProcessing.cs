using System.Reflection;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using Common.Utils.Logging;
using EventSourcingService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Services;

public class ESLogCommitProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public ESLogCommitProcessing(IPublishEndpoint publishEndpoint, EventSourcingDbContext dbContext,
        IConfiguration configuration)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task CommitESLog(ConsumeContext<ESContract> context)
    {
        try
        {
            await _dbContext.commit_operation(context.Message.CorrelationId, context.Message.IsError).FirstOrDefaultAsync();
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
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "EventSourcingService_ESLogCommit");
            messageObject.GetType().GetProperty("UserLogin").SetValue(messageObject, context.Message.UserLogin);
            messageObject.GetType().GetProperty("ItemId").SetValue(messageObject, context.Message.AuctionId);


            var faultType = typeof(FaultMessage<>);
            var typeParams = new Type[] { messageObject.GetType() };
            var faultObjectType = faultType.MakeGenericType(typeParams);

            var faultObject = Activator.CreateInstance(faultObjectType, new object[] { messageObject });

            await _publishEndpoint.Publish(faultObject.GetType().GetMethod("CastItem").Invoke(faultObject, null));
        }
    }
}