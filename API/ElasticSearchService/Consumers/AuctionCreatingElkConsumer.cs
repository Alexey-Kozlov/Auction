using Contracts;
using MassTransit;

namespace ElasticSearchService.Consumers;

public class AuctionCreatingElkConsumer : IConsumer<AuctionCreatingElk>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionCreatingElkConsumer(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<AuctionCreatingElk> consumeContext)
    {
        await _publishEndpoint.Publish(new AuctionCreatedElk(consumeContext.Message.CorrelationId));
        Console.WriteLine("--> Получение сообщения создать аукцион");
    }
}
