using Common.Contracts;
using GatewayService.Cache;
using MassTransit;

namespace GatewayService.Consumers;

public class AuctionDeletingGatewayConsumer : IConsumer<AuctionDeletingGateway>
{
    private readonly ImageCache _cacheService;
    private readonly IPublishEndpoint _publishEndpoint;
    public AuctionDeletingGatewayConsumer(ImageCache cacheService, IPublishEndpoint publishEndpoint)
    {
        _cacheService = cacheService;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<AuctionDeletingGateway> context)
    {
        Console.WriteLine("--> Получение сообщения обновить аукцион, сбрасываем кеш изображения");
        await _cacheService.DeleteCacheItem(context.Message.Id.ToString());
        await _publishEndpoint.Publish(new AuctionDeletedGateway(context.Message.CorrelationId));
    }
}
