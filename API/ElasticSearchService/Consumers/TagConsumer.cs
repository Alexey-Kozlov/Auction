using System.Reflection;
using System.Text.Json;
using Common.Contracts.Processing;
using Common.Contracts.Tag;
using Common.Utils;
using Common.Utils.Logging;
using ElasticSearchService.DTO;
using ElasticSearchService.Services;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;

namespace ElasticSearchService.Consumers;

public class TagConsumer : IConsumer<DataForProcessingServicesList<TagItem>>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ElkClient _client;
    private readonly IConfiguration _configuration;
    private static readonly AwaitLocker _locker = new AwaitLocker();
    private readonly IDistributedCache _cache;

    public TagConsumer(IPublishEndpoint publishEndpoint, ElkClient client, IConfiguration configuration,
        IDistributedCache cache)
    {
        _publishEndpoint = publishEndpoint;
        _client = client;
        _configuration = configuration;
        _cache = cache;
    }
    //получение запросов на обновление записей тегов
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<TagItem>> context)
    {
        await _locker.LockAsync(async () =>
        {
            try
            {
                var correlationId = context.Message.CorrelationId;
                foreach (var item in context.Message.DataObjects)
                {
                    var _typedItem = JsonSerializer.Deserialize<TagItem>(item.Data);
                    var typedItem = new TagSearch
                    {
                        AuctionId = _typedItem.AuctionId,
                        ItemId = _typedItem.ItemId,
                        Tag = _typedItem.Tag
                    };
                    var search = await _client.TagClient.SearchAsync<TagSearch>(indices: "tag_index",
                        p => p.Query(q => q.Match(m => m.Field(f => f.ItemId).Query(typedItem.ItemId))));
                    if (item.CRUD != CRUD.Create)
                    {
                        if (search == null) throw new Exception($"Ошибка обновления записи в елке - не найден тег с Id - {typedItem.ItemId}");
                    }
                    //сохраняем в редисе прежнюю запись, чтобы при откате можно было ее восстановить
                    if (search != null)
                    {
                        var oldAuction = search.Documents.FirstOrDefault();
                        var cacheDto = new CacheTagDTO
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
                            var response = await _client.TagClient.DeleteByQueryAsync<TagSearch>(indices: "tag_index",
                                p => p.Query(q => q.Match(m => m.Field(f => f.ItemId).Query(typedItem.ItemId)))
                                .WaitForCompletion(true).Refresh());
                            break;
                        case CRUD.Create:
                            //добавляем запись в индекс
                            await _client.TagClient.IndexAsync(typedItem, p => p.Index("tag_index"));
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
                messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "ElkService_Tag");
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
