using Contracts;
using GatewayService.Cache;
using MassTransit;

namespace GatewayService.Consumers;

public class AuctionUpdatingGatewayConsumer : IConsumer<AuctionUpdatingGateway>
{
    private readonly ImageCache _cacheService;
    private readonly IPublishEndpoint _publishEndpoint;
    public AuctionUpdatingGatewayConsumer(ImageCache cacheService, IPublishEndpoint publishEndpoint)
    {
        _cacheService = cacheService;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<AuctionUpdatingGateway> context)
    {
        Console.WriteLine("--> Получение сообщения обновить аукцион, сбрасываем кеш изображения, если оно есть в кеше");
        await _cacheService.DeleteCacheItem(context.Message.Id.ToString());
        await _publishEndpoint.Publish(new AuctionUpdatedGateway(context.Message.CorrelationId));
    }
}
