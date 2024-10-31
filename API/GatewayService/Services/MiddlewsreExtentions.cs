using Common.Utils;
using GatewayService.Cache;
using GatewayService.Models;

namespace GatewayService.Services;
public static class MiddlewsreExtentions
{
    public static IEndpointRouteBuilder ImageMiddleware(this IEndpointRouteBuilder app)
    {
        //получаем изображения из кеша - сервис с использованием reddis
        //это штатный функционал
        app.MapGet("/api/images/{auctionid}", async (string auctionid, ImageCache imageCache) =>
        {
            var img = await imageCache.GetImage(auctionid);
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
            var img = await imageCache.GetImage(auctionid);
            return Results.File(Convert.FromBase64String(img), contentType: "image/png");
        });

        return app;
    }
}