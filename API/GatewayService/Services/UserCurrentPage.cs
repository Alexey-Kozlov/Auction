using System.Text.Json;
using Common.Utils;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace GatewayService.Services;

//для сохранения в кеше редиса данных о последней посещенной странице пользователя
//используется для персонализированной рассылки обновления интерфейса - для CommunicationService
// и для BidService - для обновления сообщений чатов и ставок online
public class UserCurrentPage
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _redis;
    private readonly IConfiguration _config;
    private static readonly AwaitLocker _locker = new AwaitLocker();

    public UserCurrentPage(IDistributedCache cache, IConfiguration config, IConnectionMultiplexer redis)
    {
        _cache = cache;
        _config = config;
        _redis = redis;
    }

    public async Task SetUserForCurrentPage(string userLogin, string page)
    {
        await _locker.LockAsync(async () =>
        {
            //сохраняем новую страницу пользователя
            await _cache.SetStringAsync(
                $"CurrentPage_{userLogin}", page,
                new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromHours(Double.Parse(_config["rd:PageExpirationHours"]))
                }
                );
            //удаляем старые записи о посещении страниц пользователем
            var redis = _redis.GetServer(_redis.GetEndPoints().Single());
            var keys = redis.Keys(pattern: $"*\"_{userLogin}_");
            if (keys != null && keys.Count() > 0)
            {
                foreach (var key in keys)
                {
                    await _cache.RemoveAsync(key);
                }
            }
            //добавляем новую запись страницы с текущим пользователем
            await _cache.SetStringAsync(
                page + "_" + userLogin + "_",
                userLogin,
                 new DistributedCacheEntryOptions
                 {
                     SlidingExpiration = TimeSpan.FromHours(Double.Parse(_config["rd:PageExpirationHours"]))
                 }
            );
        });
    }

    public async Task<List<string>> GetUsersForCurrentPage(string page)
    {
        var redis = _redis.GetServer(_redis.GetEndPoints().Single());
        var keys = redis.Keys(pattern: $"*{page}*");
        if (keys == null) return null;
        var rezult = new List<string>();
        foreach (var key in keys)
        {
            rezult.Add(await _cache.GetStringAsync(key));
        }
        return rezult;
    }
}