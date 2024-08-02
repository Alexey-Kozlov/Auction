using Common.Contracts;
using ElasticSearchService.Services;
using MassTransit;

namespace ElasticSearchService.Consumers;

public class AuctionDeletingElkConsumer : IConsumer<AuctionDeletingElk>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ElkClient _client;

    public AuctionDeletingElkConsumer(IPublishEndpoint publishEndpoint, ElkClient client)
    {
        _publishEndpoint = publishEndpoint;
        _client = client;
    }
    public async Task Consume(ConsumeContext<AuctionDeletingElk> consumeContext)
    {

        var response = await _client.Client.DeleteAsync(index: "search_index",
            consumeContext.Message.Id.ToString());

        if (response.IsValidResponse)
        {
            Console.WriteLine($"Deleted document with ID {response.Id} succeeded.");
        }
        else
        {
            Console.WriteLine(response.ElasticsearchServerError);
        }
        await _publishEndpoint.Publish(new AuctionDeletedElk(consumeContext.Message.CorrelationId));
        Console.WriteLine("--> Получение сообщения удалить аукцион");
    }
}
