using System.Reflection;
using System.Text.Json;
using AutoMapper;
using Common.Contracts.Auction;
using Common.Contracts.Communication;
using Common.Contracts.Processing;
using Common.Contracts.Tag;
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
                    if (item.DataType == "AuctionItem")
                    {
                        var typedItem = JsonSerializer.Deserialize<AuctionItem>(item.Data);
                        var searchAuction = await _client.Client.SearchAsync<AuctionCreatingElk>(indices: "search_index",
                            p => p.Query(q => q.Match(m => m.Field(f => f.ItemId).Query(typedItem.ItemId))));
                        if (item.CRUD != CRUD.Create)
                        {
                            if (searchAuction == null) throw new Exception($"Ошибка обновления записи в елке - не найден аукцион с Id - {typedItem.ItemId}");
                        }
                        //сохраняем в редисе прежние записи (аукциона и сообщений пользователей), 
                        // чтобы при откате можно было их восстановить
                        if (searchAuction != null)
                        {
                            var oldAuction = searchAuction.Documents.FirstOrDefault();
                            var cacheDto = new CacheElkDTO
                            {
                                Record = oldAuction,
                                CRUD = item.CRUD
                            };
                            await _cache.SetStringAsync(correlationId.ToString(), JsonSerializer.Serialize(cacheDto, cacheDto.GetType()));
                        }
                        //начинаем обновлять запись аукциона в елке
                        var elkItem = _mapper.Map<AuctionCreatingElk>(typedItem);
                        switch (item.CRUD)
                        {
                            case CRUD.Delete:
                                //удаляем из индекса заданную запись аукциона
                                await _client.Client.DeleteByQueryAsync<AuctionCreatingElk>(indices: "search_index",
                                    p => p.Query(q => q.Match(m => m.Field(f => f.ItemId).Query(typedItem.ItemId)))
                                    .WaitForCompletion(true).Refresh());
                                break;
                            case CRUD.Create:
                                //добавляем запись в индекс
                                await _client.Client.IndexAsync(elkItem, p => p.Index("search_index"));
                                break;
                            case CRUD.Update:
                                //обновляем запись по полям - title, properties, description, auctionEnd
                                await _client.Client.UpdateByQueryAsync<AuctionCreatingElk>(indices: "search_index",
                                    p => p.Query(q => q.Match(m => m.Field(f => f.ItemId).Query(typedItem.ItemId)))
                                    .Script(s => s.Source(
                                        "ctx._source.title = params.title;" +
                                        "ctx._source.properties = params.properties;" +
                                        "ctx._source.description = params.description;" +
                                        "ctx._source.auctionEnd = params.auctionEnd;"
                                    ).Params(p => p
                                    .Add("title", elkItem.Title)
                                    .Add("properties", elkItem.Properties)
                                    .Add("description", elkItem.Description)
                                    .Add("auctionEnd", elkItem.AuctionEnd)))
                                    .Conflicts(Conflicts.Proceed)
                                    .WaitForCompletion(true).Refresh());
                                break;
                        }
                    }

                    //сообщения пользователей
                    if (item.DataType == "CommunicationItem")
                    {
                        var typedItem = JsonSerializer.Deserialize<CommunicationItem>(item.Data);
                        var searchChats = await _client.CommunicationClient.SearchAsync<CommunicationSearch>(indices: "communication_index",
                            p => p.Query(q => q.Match(m => m.Field(f => f.AuctionId).Query(typedItem.ItemId))));
                        //для сообщений пользователей - сохраняем в кеше при удалении аукциона
                        if (item.CRUD == CRUD.Delete && searchChats != null)
                        {
                            foreach (var chatItem in searchChats.Documents)
                            {
                                var cacheDto = new CacheCommunicationDTO
                                {
                                    Record = chatItem,
                                    CRUD = item.CRUD
                                };
                                await _cache.SetStringAsync(correlationId.ToString(), JsonSerializer.Serialize(cacheDto, cacheDto.GetType()));
                            }
                        }
                        var elkItem = _mapper.Map<CommunicationSearch>(typedItem);
                        //при переиндексации - добавляем в индекс communication_index
                        if (item.CRUD == CRUD.Create)
                        {
                            await _client.CommunicationClient.IndexAsync(elkItem, p => p.Index("communication_index"));
                        }
                        //при удалении аукциона - удаляем из индекса чаты этого аукциона
                        if (item.CRUD == CRUD.Delete && searchChats != null)
                        {
                            foreach (var chatItem in searchChats.Documents)
                            {
                                await _client.CommunicationClient.DeleteByQueryAsync<CommunicationSearch>(indices: "communication_index",
                                p => p.Query(q => q.Match(m => m.Field(f => f.ItemId).Query(chatItem.ItemId)))
                                .WaitForCompletion(true).Refresh());
                            }
                        }
                    }

                    //теги
                    if (item.DataType == "TagItem")
                    {
                        var typedItem = JsonSerializer.Deserialize<TagItem>(item.Data);
                        var searchTags = await _client.TagClient.SearchAsync<TagSearch>(indices: "tag_index",
                            p => p.Query(q => q.Match(m => m.Field(f => f.AuctionId).Query(typedItem.ItemId))));
                        if (item.CRUD == CRUD.Delete && searchTags != null)
                        {
                            foreach (var chatItem in searchTags.Documents)
                            {
                                var cacheDto = new CacheTagDTO
                                {
                                    Record = chatItem,
                                    CRUD = item.CRUD
                                };
                                await _cache.SetStringAsync(correlationId.ToString(), JsonSerializer.Serialize(cacheDto, cacheDto.GetType()));
                            }
                        }
                        var elkItem = _mapper.Map<TagSearch>(typedItem);
                        //при переиндексации - добавляем в индекс tag_index
                        if (item.CRUD == CRUD.Create)
                        {
                            await _client.TagClient.IndexAsync(elkItem, p => p.Index("tag_index"));
                        }
                        //при удалении аукциона - удаляем из индекса теги этого аукциона
                        if (item.CRUD == CRUD.Delete && searchTags != null)
                        {
                            foreach (var tagItem in searchTags.Documents)
                            {
                                await _client.TagClient.DeleteByQueryAsync<TagSearch>(indices: "tag_index",
                                p => p.Query(q => q.Match(m => m.Field(f => f.ItemId).Query(tagItem.ItemId)))
                                .WaitForCompletion(true).Refresh());
                            }
                        }
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
