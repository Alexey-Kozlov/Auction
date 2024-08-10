using Common.Contracts;
using ElasticSearchService.Services;
using MassTransit;

namespace ElasticSearchService.Consumers;

public class ElkIndexConsumer : IConsumer<ElkIndexCreating>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ElkClient _client;

    public ElkIndexConsumer(IPublishEndpoint publishEndpoint, ElkClient client)
    {
        _publishEndpoint = publishEndpoint;
        _client = client;
    }
    public async Task Consume(ConsumeContext<ElkIndexCreating> consumeContext)
    {
        var elkResponse = await _client.Client.SearchAsync<AuctionCreatingElk>(s =>
            s.Query(q => q.Ids(i => i.Values(consumeContext.Message.Item.Id.ToString())))
        );

        if (elkResponse.IsValidResponse)
        {
            if (elkResponse.Documents.Count > 0)
            {
                //обновление индекса
                Console.WriteLine($"{DateTime.Now} - Обновление документа {consumeContext.Message.ItemNumber} {consumeContext.Message.Item.Title}");
                await _client.Client.UpdateAsync<AuctionCreatingSearch, AuctionCreatingElk>(
                    consumeContext.Message.Item.Id.ToString(),
                    p => p.Doc(consumeContext.Message.Item));
            }
            else
            {
                //создание индекса
                Console.WriteLine($"{DateTime.Now} - Новый документ {consumeContext.Message.Item.Title}");
                await _client.Client.IndexAsync(consumeContext.Message.Item);
            }
            await _publishEndpoint.Publish(new ElkIndexCreated(consumeContext.Message.CorrelationId, ResultType.Success));
        }
        else
        {
            await _publishEndpoint.Publish(new ElkIndexCreated(consumeContext.Message.CorrelationId, ResultType.Error));
            throw new Exception("Ошибка ELK-индексатора");
        }
    }
}
