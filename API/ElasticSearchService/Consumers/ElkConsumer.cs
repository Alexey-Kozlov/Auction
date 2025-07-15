using System.Reflection;
using System.Text.Json;
using AutoMapper;
using Common.Contracts.Auction;
using Common.Contracts.Processing;
using Common.Utils;
using Common.Utils.Logging;
using Elastic.Clients.Elasticsearch;
using ElasticSearchService.DTO;
using ElasticSearchService.Services;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;

namespace ElasticSearchService.Consumers;

public class ElkConsumer : IConsumer<DataForProcessingServicesList<AuctionItem>>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ElkClient _client;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private static readonly AwaitLocker _locker = new AwaitLocker();
    private readonly IDistributedCache _cache;

    public ElkConsumer(IPublishEndpoint publishEndpoint, ElkClient client, IConfiguration configuration,
        IMapper mapper, IDistributedCache cache)
    {
        _publishEndpoint = publishEndpoint;
        _client = client;
        _configuration = configuration;
        _mapper = mapper;
        _cache = cache;
    }
    //получение запросов на обновление записей, переиндексацию
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<AuctionItem>> context)
    {
        await _locker.LockAsync(async () =>
        {
            try
            {
                var correlationId = context.Message.CorrelationId;

                foreach (var item in context.Message.DataObjects)
                {
                    var typedItem = JsonSerializer.Deserialize<AuctionItem>(item.Data);
                    var search = await _client.Client.SearchAsync<AuctionCreatingElk>(indices: "search_index",
                        p => p.Query(q => q.Match(m => m.Field(f => f.ItemId).Query(typedItem.ItemId))));
                    if (item.CRUD != CRUD.Create)
                    {
                        if (search == null) throw new Exception($"Ошибка обновления записи в елке - не найден аукцион с Id - {typedItem.ItemId}");
                    }
                    //сохраняем в редисе прежнюю запись, чтобы при откате можно было ее восстановить
                    if (search != null)
                    {
                        var oldAuction = search.Documents.FirstOrDefault();
                        var cacheDto = new CacheElkDTO
                        {
                            Record = oldAuction,
                            CRUD = item.CRUD
                        };
                        await _cache.SetStringAsync(correlationId.ToString(), JsonSerializer.Serialize(cacheDto, cacheDto.GetType()));
                    }
                    //начинаем обновлять запись в елке
                    var elkItem = _mapper.Map<AuctionCreatingElk>(typedItem);
                    switch (item.CRUD)
                    {
                        case CRUD.Delete:
                            //удаляем из индекса заданную запись
                            var response = await _client.Client.DeleteByQueryAsync<AuctionCreatingElk>(indices: "search_index",
                                p => p.Query(q => q.Match(m => m.Field(f => f.ItemId).Query(typedItem.ItemId)))
                                .WaitForCompletion(true).Refresh());
                            break;
                        case CRUD.Create:
                            //добавляем запись в индекс
                            await _client.Client.IndexAsync(elkItem, p => p.Index("search_index"));
                            break;
                        case CRUD.Update:
                            //обновляем запись по полям - title, properties, description
                            await _client.Client.UpdateByQueryAsync<AuctionCreatingElk>(indices: "search_index",
                                p => p.Query(q => q.Match(m => m.Field(f => f.ItemId).Query(typedItem.ItemId)))
                                .Script(s => s.Source(
                                    "ctx._source.title = params.title;" +
                                    "ctx._source.properties = params.properties;" +
                                    "ctx._source.description = params.description;"
                                ).Params(p => p
                                .Add("title", elkItem.Title)
                                .Add("properties", elkItem.Properties)
                                .Add("description", elkItem.Description)))
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
                messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "ElkService_ELK");
                messageObject.GetType().GetProperty("UserLogin").SetValue(messageObject, "");
                messageObject.GetType().GetProperty("IsError").SetValue(messageObject, true);
                var faultType = typeof(FaultMessage<>);
                var typeParams = new Type[] { messageObject.GetType() };
                var faultObjectType = faultType.MakeGenericType(typeParams);
                var faultObject = Activator.CreateInstance(faultObjectType, new object[] { messageObject });

                await _publishEndpoint.Publish(faultObject.GetType().GetMethod("CastItem").Invoke(faultObject, null));
            }
        });
    }
}
