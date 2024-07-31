using Contracts;
using ElasticSearchService.Services;
using MassTransit;

namespace ElasticSearchService.Consumers;

public class AuctionUpdatingElkConsumer : IConsumer<AuctionUpdatingElk>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ElkClient _client;

    public AuctionUpdatingElkConsumer(IPublishEndpoint publishEndpoint, ElkClient client)
    {
        _publishEndpoint = publishEndpoint;
        _client = client;
    }
    public async Task Consume(ConsumeContext<AuctionUpdatingElk> consumeContext)
    {
        //обновляем документ
        var response = await _client.Client.UpdateAsync<AuctionCreatingSearch, AuctionUpdatingElk>(
            index: "search_index",
            consumeContext.Message.Id.ToString(),
            p => p.Doc(consumeContext.Message));
        if (response.IsValidResponse)
        {
            Console.WriteLine($"Updated document with ID {response.Id} succeeded.");
        }
        else
        {
            Console.WriteLine(response.ElasticsearchServerError);
        }
        await _publishEndpoint.Publish(new AuctionUpdatedElk(consumeContext.Message.CorrelationId));
        Console.WriteLine("--> Получение сообщения обновить аукцион");
    }
}
