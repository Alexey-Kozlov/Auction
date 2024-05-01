using Contracts;
using GatewayService.Cache;
using MassTransit;

namespace GatewayService.Consumers;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
{
    private readonly ImageCache _cacheService;
    public AuctionUpdatedConsumer(ImageCache cacheService)
    {
        _cacheService = cacheService;
    }
    public async Task Consume(ConsumeContext<AuctionUpdated> consumeContext)
    {

        if (string.IsNullOrEmpty(consumeContext.Message.Image)) return;
        Console.WriteLine("--> Получение сообщения обновить аукцион, сбрасываем кеш изображения");
        await _cacheService.DeleteCacheItem(consumeContext.Message.Id);
    }
}
