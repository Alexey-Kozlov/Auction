using Common.Contracts;
using GatewayService.Cache;
using GatewayService.Models;
using Microsoft.AspNetCore.Mvc;

namespace GatewayService.Services;
public static class ImageMiddlewareExtentions
{
    public static IEndpointRouteBuilder ImageMiddleware(this IEndpointRouteBuilder app)
    {
        //получаем изображения из кеша - сервис с использованием reddis
        //это штатный функционал
        app.MapGet("/api/images",
            async ([FromQuery(Name = "id")] string auctionid,
            [FromQuery(Name = "cache")] bool cache,
            ImageCache imageCache) =>
        {
            var img = await imageCache.GetImage(auctionid, cache);
            return new ApiResponse<ImageDTO>()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccess = true,
                Result = new ImageDTO(auctionid, img)
            };
        });

        //дополнительный функционал - можно вызывать изображение в виде файла - 
        //для штатного вызова из HTML, например, через <img src="адрес данного сервиса">
        app.MapGet("/api/images_file/{auctionid}", async (string auctionid, ImageCache imageCache) =>
        {
            var img = await imageCache.GetImage(auctionid, false);
            return Results.File(Convert.FromBase64String(img), contentType: "image/png");
        });

        return app;
    }
}