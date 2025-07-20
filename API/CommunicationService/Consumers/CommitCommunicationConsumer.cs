using System.Reflection;
using Common.Contracts.Communication;
using Common.Contracts.Processing;
using Common.Utils.Logging;
using CommunicationService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CommunicationService.Consumers;

public class CommitCommunicationConsumer : IConsumer<CommunicationCommit>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;
    private readonly CommunicationDbContext _dbContext;

    public CommitCommunicationConsumer(IPublishEndpoint publishEndpoint,
        IConfiguration configuration, CommunicationDbContext dbContext)
    {
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
        _dbContext = dbContext;
    }
    public async Task Consume(ConsumeContext<CommunicationCommit> context)
    {
        var correlationId = context.Message.CorrelationId;
        try
        {
            await CommitItems(correlationId, context.Message.Commited);
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
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "CommunicationService_Commit");
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

    public async Task CommitItems(Guid correlationId, bool commit)
    {
        var correlationid_par = new NpgsqlParameter("correlationid_par", System.Data.DbType.Guid);
        correlationid_par.Direction = System.Data.ParameterDirection.Input;
        correlationid_par.Value = correlationId;
        var reset_par = new NpgsqlParameter("reset_par", System.Data.DbType.Boolean);
        reset_par.Direction = System.Data.ParameterDirection.Input;
        reset_par.Value = false;
        var commit_par = new NpgsqlParameter("commit_par", System.Data.DbType.Boolean);
        commit_par.Direction = System.Data.ParameterDirection.Input;
        commit_par.Value = commit;
        await _dbContext.Database.ExecuteSqlRawAsync("call communication_commit_proc({0},{1},{2})",
            correlationid_par,
            reset_par,
            commit_par);
    }
}
