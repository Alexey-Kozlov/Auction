using Contracts;
using GatewayService.Cache;
using MassTransit;

namespace GatewayService.Consumers;

public class ImageUpdatedConsumer : IConsumer<ImageUpdated>
{
    private readonly ImageCache _cacheService;
    public ImageUpdatedConsumer(ImageCache cacheService)
    {
        _cacheService = cacheService;
    }
    public async Task Consume(ConsumeContext<ImageUpdated> consumeContext)
    {

        if (string.IsNullOrEmpty(consumeContext.Message.id)) return;
        Console.WriteLine("--> Получение сообщения обновить аукцион, сбрасываем кеш изображения");
        await _cacheService.DeleteCacheItem(consumeContext.Message.id);
    }
}
