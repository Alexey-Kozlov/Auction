using GatewayService.Models;
using GatewayService.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace GatewayService.Cache;

public class ImageCache
{
    private readonly IDistributedCache _cache;
    private readonly GrpcImageClient _client;
    private readonly IConfiguration _config;
    public ImageCache(IDistributedCache cache, GrpcImageClient client, IConfiguration config)
    {
        _cache = cache;
        _client = client;
        _config = config;
    }

    public async Task<string> GetImage(string imageId)
    {
        var _image = await _cache.GetStringAsync(imageId);
        ImageDTO imageDto = null;
        if (_image == null)
        {
            Console.WriteLine($"{DateTime.Now} - Изображение {imageId} НЕ найдено в кеше, получаем из микросервиса ImageService");
            imageDto = await _client.GetImage(imageId);
            if (string.IsNullOrEmpty(imageDto.Image))
            {
                Console.WriteLine($"{DateTime.Now} - Изображение {imageId} не найдено в GRPC Image");
                return "";
            }
            await _cache.SetStringAsync(imageId, imageDto.Image, new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromDays(Double.Parse(_config["CacheImageExpirationDays"]))
            });
            _image = imageDto.Image;
            Console.WriteLine($"{DateTime.Now} - Изображение {imageId} записано и извлечено из кеша");
        }
        else
        {
            Console.WriteLine($"{DateTime.Now} - Изображение {imageId} извлечено из кеша");
        }
        return _image ?? "не найдено";
    }

    public async Task DeleteCacheItem(string key)
    {
        var _image = await _cache.GetStringAsync(key);
        if (_image != null)
        {
            await _cache.RemoveAsync(key);
        }
    }
}