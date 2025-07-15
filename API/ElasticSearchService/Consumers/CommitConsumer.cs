using System.Reflection;
using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Communication;
using Common.Contracts.ELKSearch;
using Common.Contracts.Processing;
using Common.Utils.Logging;
using Elastic.Clients.Elasticsearch;
using ElasticSearchService.DTO;
using ElasticSearchService.Services;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;

namespace ElasticSearchService.Consumers;

public class CommitConsumer : IConsumer<ElkCommit>
{
    private readonly ElkClient _client;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;
    private readonly IDistributedCache _cache;

    public CommitConsumer(ElkClient client, IPublishEndpoint publishEndpoint, IConfiguration configuration,
        IDistributedCache cache)
    {
        _client = client;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
        _cache = cache;
    }
    public async Task Consume(ConsumeContext<ElkCommit> context)
    {

        var correlationId = context.Message.CorrelationId;
        try
        {
            var search = await _cache.GetStringAsync(context.Message.CorrelationId.ToString());
            //если был откат транзакции - восстанавливаем информацию до транзакции 
            // из ранее сохраненный в кеше  

            //для функционала аукциона 
            if (context.Message.ElkIndex == "search_index")
            {
                if (!context.Message.Commited && !string.IsNullOrEmpty(search))
                {
                    var typedItem = JsonSerializer.Deserialize<CacheElkDTO>(search);
                    //если было обновление или удаление записи - восстанавливаем запись из кеша редиса
                    if (typedItem.CRUD != CRUD.Create)
                    {
                        await _client.Client.UpdateByQueryAsync<AuctionCreatingElk>(indices: context.Message.ElkIndex,
                            p => p.Query(q => q.Match(m => m.Field(f => f.ItemId).Query(typedItem.Record.ItemId)))
                            .Script(s => s.Source(
                            "ctx._source.title = params.title;" +
                            "ctx._source.properties = params.properties;" +
                            "ctx._source.description = params.description;"
                        ).Params(p => p
                        .Add("title", typedItem.Record.Title)
                        .Add("properties", typedItem.Record.Properties)
                        .Add("description", typedItem.Record.Description)))
                        .Conflicts(Conflicts.Proceed)
                        .WaitForCompletion(true).Refresh());
                    }
                    else
                    {
                        //если было создание - удаляем созданную запись из елки
                        var response = await _client.Client.DeleteByQueryAsync<AuctionCreatingElk>(indices: context.Message.ElkIndex,
                            p => p.Query(q => q.Match(m => m.Field(f => f.ItemId).Query(typedItem.Record.ItemId)))
                            .WaitForCompletion(true).Refresh());
                    }

                }
            }

            //для функционала чата
            if (context.Message.ElkIndex == "communication_index")
            {
                if (!context.Message.Commited && !string.IsNullOrEmpty(search))
                {
                    var typedItem = JsonSerializer.Deserialize<CacheCommunicationDTO>(search);
                    //если было обновление или удаление записи - восстанавливаем запись из кеша редиса
                    if (typedItem.CRUD != CRUD.Create)
                    {
                        await _client.CommunicationClient.UpdateByQueryAsync<CommunicationSearch>(indices: context.Message.ElkIndex,
                            p => p.Query(q => q.Match(m => m.Field(f => f.ItemId).Query(typedItem.Record.ItemId)))
                            .Script(s => s.Source(
                            "ctx._source.message = params.message;"
                        ).Params(p => p
                        .Add("message", typedItem.Record.Message)))
                        .Conflicts(Conflicts.Proceed)
                        .WaitForCompletion(true).Refresh());
                    }
                    else
                    {
                        //если было создание - удаляем созданную запись из елки
                        var response = await _client.Client.DeleteByQueryAsync<CommunicationSearch>(indices: context.Message.ElkIndex,
                            p => p.Query(q => q.Match(m => m.Field(f => f.ItemId).Query(typedItem.Record.ItemId)))
                            .WaitForCompletion(true).Refresh());
                    }
                }
            }

            //общая часть
            if (!string.IsNullOrEmpty(search))
            {
                await _cache.RemoveAsync(context.Message.CorrelationId.ToString());
            }
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
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "ElasticSearch_Commit");
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
