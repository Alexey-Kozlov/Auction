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
        app.MapGet("/api/images/{id}", async (string id, ImageCache imageCache) =>
        {
            var img = await imageCache.GetImage(id);
            return new ApiResponse<ImageDTO>()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccess = true,
                Result = new ImageDTO(id, img)
            };
        });

        //дополнительный функционал - можно вызывать изображение в виде файла - 
        //для штатного вызова из HTML, например, через <img src="адрес данного сервиса">
        app.MapGet("/api/images_file/{id}", async (string id, ImageCache imageCache) =>
        {
            var img = await imageCache.GetImage(id);
            return Results.File(Convert.FromBase64String(img), contentType: "image/png");
        });

        return app;
    }
}