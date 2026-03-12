using System.Reflection;
using Common.Contracts.Auction;
using Common.Contracts.Communication;
using Common.Contracts.ELKSearch;
using Common.Contracts.Processing;
using Common.Contracts.Tag;
using Common.Utils.Logging;
using ElasticSearchService.Services;
using MassTransit;

namespace ElasticSearchService.Consumers;

public class ResetElkConsumer : IConsumer<ElkIndexResetRequest>
{
    private readonly ElkClient _client;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;


    public ResetElkConsumer(IPublishEndpoint publishEndpoint, ElkClient client,
        IConfiguration configuration)
    {
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
        _client = client;
    }
    public async Task Consume(ConsumeContext<ElkIndexResetRequest> context)
    {
        var correlationId = context.Message.CorrelationId;
        try
        {
            //сбрасываем БД поиска аукциона
            await _client.Client.DeleteByQueryAsync<AuctionCreatingElk>(indices: "search_index",
                p => p.Query(q => q.QueryString(f => f.Query("*"))));

            //сбрасываем БД поиска чатов
            await _client.CommunicationClient.DeleteByQueryAsync<CommunicationSearch>(indices: "communication_index",
                p => p.Query(q => q.QueryString(f => f.Query("*"))));

            //сбрасываем БД поиска тегов
            await _client.TagClient.DeleteByQueryAsync<TagSearch>(indices: "tag_index",
                p => p.Query(q => q.QueryString(f => f.Query("*"))));

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
            messageObject.GetType().GetProperty("ErrorMessage").SetValue(messageObject, GetErrorMessage.GetInnerException(e).Message);
            messageObject.GetType().GetProperty("ErrorExceptionMessage").SetValue(messageObject, e.StackTrace);
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "ElkService_Reset");
            messageObject.GetType().GetProperty("UserLogin").SetValue(messageObject, "");
            messageObject.GetType().GetProperty("IsError").SetValue(messageObject, true);
            var faultType = typeof(FaultMessage<>);
            var typeParams = new Type[] { messageObject.GetType() };
            var faultObjectType = faultType.MakeGenericType(typeParams);
            var faultObject = Activator.CreateInstance(faultObjectType, new object[] { messageObject });

            await _publishEndpoint.Publish(faultObject.GetType().GetMethod("CastItem").Invoke(faultObject, null));
        }
    }
}
