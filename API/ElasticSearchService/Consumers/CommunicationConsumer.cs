using System.Reflection;
using System.Text.Json;
using Common.Contracts.Communication;
using Common.Contracts.Processing;
using Common.Utils;
using Common.Utils.Logging;
using Elastic.Clients.Elasticsearch;
using ElasticSearchService.DTO;
using ElasticSearchService.Services;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;

namespace ElasticSearchService.Consumers;

public class CommunicationConsumer : IConsumer<DataForProcessingServicesList<CommunicationItem>>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ElkClient _client;
    private readonly IConfiguration _configuration;
    private static readonly AwaitLocker _locker = new AwaitLocker();
    private readonly IDistributedCache _cache;

    public CommunicationConsumer(IPublishEndpoint publishEndpoint, ElkClient client, IConfiguration configuration,
        IDistributedCache cache)
    {
        _publishEndpoint = publishEndpoint;
        _client = client;
        _configuration = configuration;
        _cache = cache;
    }
    //получение запросов на обновление записей чатов
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<CommunicationItem>> context)
    {
        await _locker.LockAsync(async () =>
        {
            try
            {
                var correlationId = context.Message.CorrelationId;
                foreach (var item in context.Message.DataObjects)
                {
                    var _typedItem = JsonSerializer.Deserialize<CommunicationItem>(item.Data);
                    var typedItem = new CommunicationSearch
                    {
                        AuctionId = _typedItem.AuctionId.Value,
                        ItemId = _typedItem.ItemId.Value,
                        Message = _typedItem.Message
                    };
                    var search = await _client.CommunicationClient.SearchAsync<CommunicationSearch>(indices: "communication_index",
                        p => p.Query(q => q.Match(m => m.Field(f => f.ItemId).Query(typedItem.ItemId))));
                    if (item.CRUD != CRUD.Create)
                    {
                        if (search == null) throw new Exception($"Ошибка обновления записи в елке - не найден чат с Id - {typedItem.ItemId}");
                    }
                    //сохраняем в редисе прежнюю запись, чтобы при откате можно было ее восстановить
                    if (search != null)
                    {
                        var oldAuction = search.Documents.FirstOrDefault();
                        var cacheDto = new CacheCommunicationDTO
                        {
                            Record = oldAuction,
                            CRUD = item.CRUD
                        };
                        await _cache.SetStringAsync(correlationId.ToString(), JsonSerializer.Serialize(cacheDto, cacheDto.GetType()));
                    }
                    //начинаем обновлять запись в елке
                    switch (item.CRUD)
                    {
                        case CRUD.Delete:
                            //удаляем из индекса заданную запись
                            var response = await _client.CommunicationClient.DeleteByQueryAsync<CommunicationSearch>(indices: "communication_index",
                                p => p.Query(q => q.Match(m => m.Field(f => f.ItemId).Query(typedItem.ItemId)))
                                .WaitForCompletion(true).Refresh());
                            break;
                        case CRUD.Create:
                            //добавляем запись в индекс
                            await _client.CommunicationClient.IndexAsync(typedItem, p => p.Index("communication_index"));
                            break;
                        case CRUD.Update:
                            //обновляем запись по полю - message
                            await _client.CommunicationClient.UpdateByQueryAsync<CommunicationSearch>(indices: "communication_index",
                                p => p.Query(q => q.Match(m => m.Field(f => f.ItemId).Query(typedItem.ItemId)))
                                .Script(s => s.Source("ctx._source.message = params.message;")
                                .Params(p => p
                                .Add("message", typedItem.Message)))
                                .Conflicts(Conflicts.Proceed)
                                .WaitForCompletion(true).Refresh());
                            break;
                    }
                }

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
                messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "ElkService_Communication");
                messageObject.GetType().GetProperty("UserLogin").SetValue(messageObject, "");

                var faultType = typeof(FaultMessage<>);
                var typeParams = new Type[] { messageObject.GetType() };
                var faultObjectType = faultType.MakeGenericType(typeParams);
                var faultObject = Activator.CreateInstance(faultObjectType, new object[] { messageObject });

                await _publishEndpoint.Publish(faultObject.GetType().GetMethod("CastItem").Invoke(faultObject, null));
            }
        });
    }
}
