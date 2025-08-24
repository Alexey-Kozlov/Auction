using GatewayService.Models;
using Microsoft.Extensions.Caching.Distributed;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace GatewayService.Services;

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

    public async Task<string> GetImage(string auctionId, bool cache)
    {
        if (!cache)
        {
            //не используем кеш (для изображений на странице просмотра аукциона)
            return _client.GetImage(auctionId).GetAwaiter().GetResult().Image;
        }
        //если используем кеш для получения изображений
        var _image = await _cache.GetStringAsync(auctionId);
        ImageDTO imageDto = null;
        if (_image == null)
        {
            imageDto = await _client.GetImage(auctionId);
            if (string.IsNullOrEmpty(imageDto.Image))
            {
                return "";
            }
            //сохраняем "усеченную" копию изображения в кеше редиса
            var originImage = Convert.FromBase64String(imageDto.Image
                        .Replace("data:image/png;base64,", "")
                        .Replace("data:image/jpeg;base64,", "")
                        .Replace("data:image/jpg;base64,", ""));
            float x = 0;
            float y = 0;
            var cacheImageWidth = Int32.Parse(_config["CacheImageSize:Width"]);
            var cacheImageHeight = Int32.Parse(_config["CacheImageSize:Height"]);
            using (var ms = new MemoryStream(originImage))
            {
                using (var image = Image.Load(ms))
                {
                    x = image.Width;
                    y = image.Height;
                    float rate = x / y;
                    if (x > cacheImageWidth && y <= cacheImageHeight)
                    {
                        x = cacheImageWidth;
                        y = Convert.ToInt32(x / rate);
                    }
                    if (x <= cacheImageWidth && y > cacheImageHeight)
                    {
                        y = cacheImageHeight;
                        x = Convert.ToInt32(y * rate);
                    }
                    if (x > cacheImageWidth && y > cacheImageHeight)
                    {
                        //выравниваем размер по наиболее болшьшой стороне изображения
                        if (x >= y)
                        {
                            x = cacheImageWidth;
                            y = Convert.ToInt32(x / rate);
                        }
                        else
                        {
                            y = cacheImageHeight;
                            x = Convert.ToInt32(y * rate);
                        }
                    }
                    image.Mutate(p => p.Resize(Convert.ToInt32(x), Convert.ToInt32(y)));
                    using (var expStream = new MemoryStream())
                    {
                        image.Save(expStream, new JpegEncoder());
                        var newImage = Convert.ToBase64String(expStream.ToArray());
                        await _cache.SetStringAsync(auctionId, newImage,
                            new DistributedCacheEntryOptions
                            {
                                SlidingExpiration = TimeSpan.FromDays(Double.Parse(_config["rd:ImageExpirationDays"]))
                            });
                        return newImage;
                    }
                }
            }
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