using Common.Contracts;
using ElasticSearchService.Services;
using MassTransit;

namespace ElasticSearchService.Consumers;

public class AuctionFinishingElkConsumer : IConsumer<AuctionFinishingElk>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ElkClient _client;

    public AuctionFinishingElkConsumer(IPublishEndpoint publishEndpoint, ElkClient client)
    {
        _publishEndpoint = publishEndpoint;
        _client = client;
    }
    public async Task Consume(ConsumeContext<AuctionFinishingElk> consumeContext)
    {
        var response = await _client.Client.UpdateAsync<AuctionCreatingSearch, AuctionFinishingElk>(
            consumeContext.Message.Id.ToString(),
            p => p.Doc(consumeContext.Message));
        if (response.IsValidResponse)
        {
            Console.WriteLine($"Finished document with ID {response.Id} succeeded.");
        }
        else
        {
            Console.WriteLine(response.ElasticsearchServerError);
        }
        await _publishEndpoint.Publish(new AuctionCreatedElk(consumeContext.Message.CorrelationId));
        Console.WriteLine("--> Получение сообщения завершить аукцион");
    }
}
