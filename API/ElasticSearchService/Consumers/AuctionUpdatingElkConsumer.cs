using Common.Contracts.Auction;
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
            consumeContext.Message.AuctionId.ToString(),
            p => p.Doc(consumeContext.Message));
        if (!response.IsValidResponse)
        {
            Console.WriteLine(response.ElasticsearchServerError);
        }
        await _publishEndpoint.Publish(new AuctionUpdatedElk
        {
            CorrelationId = consumeContext.Message.CorrelationId
        });
    }
}
