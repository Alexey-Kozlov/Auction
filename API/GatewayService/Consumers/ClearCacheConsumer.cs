using Common.Contracts.Image;
using MassTransit;
using StackExchange.Redis;

namespace GatewayService.Consumers;

public class ClearCacheConsumer : IConsumer<ResetImageCache>
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IPublishEndpoint _publishEndpoint;

    public ClearCacheConsumer(IConnectionMultiplexer redis, IPublishEndpoint publishEndpoint)
    {
        _redis = redis;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<ResetImageCache> context)
    {
        //здесь очищаем весь кеш
        var redis = _redis.GetServer(_redis.GetEndPoints().Single());
        await redis.FlushAllDatabasesAsync();

        await _publishEndpoint.Publish(new ResetImageCacheNotification { UserLogin = context.Message.UserLogin });
    }

}