using System.Reflection;
using System.Text.Json;
using AutoMapper;
using Common.Contracts.Auction;
using Common.Contracts.Processing;
using ElasticSearchService.Services;
using MassTransit;

namespace ElasticSearchService.Consumers;

public class ElkConsumer : IConsumer<DataForProcessingServicesList<AuctionItem>>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ElkClient _client;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

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
        var correlationId = context.Message.CorrelationId;
        foreach (var item in context.Message.DataObjects)
        {
            var typedItem = JsonSerializer.Deserialize<AuctionItem>(item.Data);
            switch (item.CRUD)
            {
                case CRUD.Delete:
                    await _client.Client.DeleteByQueryAsync<AuctionCreatingElk>(indices: "search_index",
                        p => p.Query(q => q.Match(m => m.Field(f => f.AuctionId).Query(typedItem.AuctionId))));
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
                        await ProcessIndex(typedItem);
                    }
                    break;
                case CRUD.Update:
                    await ProcessIndex(typedItem);
                    break;
            }
        }

        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
        await _publishEndpoint.Publish(sendObject);
    }

    private async Task ProcessIndex(AuctionItem item)
    {
        var elkResponse = await _client.Client.SearchAsync<AuctionCreatingElk>(s =>
            s.Query(q => q.Ids(i => i.Values(item.AuctionId.ToString())))
        );
        var elkItem = _mapper.Map<AuctionCreatingElk>(item);
        if (elkResponse.IsValidResponse)
        {
            if (elkResponse.Documents.Count > 0)
            {
                //обновление индекса
                await _client.Client.UpdateAsync<AuctionCreatingSearch, AuctionCreatingElk>(
                    item.AuctionId.ToString(),
                    p => p.Doc(elkItem));
            }
            else
            {
                //создание индекса
                await _client.Client.IndexAsync(elkItem);
            }
        }
    }
}
