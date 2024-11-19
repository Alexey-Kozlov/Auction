using System.Reflection;
using System.Text.Json;
using AutoMapper;
using Common.Contracts.Auction;
using Common.Contracts.Processing;
using Common.Utils;
using ElasticSearchService.Services;
using MassTransit;

namespace ElasticSearchService.Consumers;

public class ElkConsumer : IConsumer<DataForProcessingServicesList<AuctionItem>>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ElkClient _client;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private static readonly AwaitLocker _locker = new AwaitLocker();

    public ElkConsumer(IPublishEndpoint publishEndpoint, ElkClient client, IConfiguration configuration,
        IMapper mapper)
    {
        _publishEndpoint = publishEndpoint;
        _client = client;
        _configuration = configuration;
        _mapper = mapper;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<AuctionItem>> context)
    {
        await _locker.LockAsync(async () =>
        {
            var correlationId = context.Message.CorrelationId;
            foreach (var item in context.Message.DataObjects)
            {
                var typedItem = JsonSerializer.Deserialize<AuctionItem>(item.Data);
                var elkItem = _mapper.Map<AuctionCreatingElk>(typedItem);
                switch (item.CRUD)
                {
                    case CRUD.Delete:
                        await _client.Client.DeleteByQueryAsync<AuctionCreatingElk>(indices: "search_index",
                            p => p.Query(q => q.Ids(i => i.Values(typedItem.AuctionId.ToString()))));
                        break;
                    case CRUD.Create:
                        if (item.DataType == "ElkIndexReset")
                        {
                            //переиндексация, сбрасываем всю БД поискаи возвращаемся
                            await _client.Client.DeleteByQueryAsync<AuctionCreatingElk>(indices: "search_index",
                                p => p.Query(q => q.QueryString(f => f.Query("*"))));
                        }
                        else
                        {
                            await _client.Client.IndexAsync(elkItem, elkItem.AuctionId);
                        }
                        break;
                    case CRUD.Update:
                        await _client.Client.IndexAsync(elkItem, elkItem.AuctionId);
                        break;
                }
            }

            var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
            await _publishEndpoint.Publish(sendObject);
        });
    }
}
