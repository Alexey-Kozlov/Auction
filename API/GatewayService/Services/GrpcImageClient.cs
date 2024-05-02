using GatewayService.Models;
using Grpc.Core;
using Grpc.Net.Client;
using ImageService;

namespace GatewayService.Services;

public class GrpcImageClient
{
    private readonly IConfiguration _config;

    public GrpcImageClient(IConfiguration config)
    {
        _config = config;
    }

    public async Task<ImageDTO> GetImage(string id)
    {
        Console.WriteLine($"{DateTime.Now} Вызов GrpcImage сервер");
        var channel = GrpcChannel.ForAddress(_config["GrpcImage"], new GrpcChannelOptions
        {
            MaxSendMessageSize = int.MaxValue,
            MaxReceiveMessageSize = int.MaxValue
        });
        var client = new GrpcImage.GrpcImageClient(channel);
        var request = new GetImageRequest { Id = id };

        try
        {
            var reply = await client.GetImageAsync(request);
            var imageDto = new ImageDTO(id, reply.Image.Image);
            return imageDto;
        }
        catch (RpcException ex)
        {
            if (ex.StatusCode == StatusCode.NotFound)
            {
                Console.WriteLine($"{DateTime.Now} Изображение в БД не найдено");
                return new ImageDTO(id, "");
            }
            Console.WriteLine($"{DateTime.Now} Ошибка GRPC Image - {ex.Message}");
            return new ImageDTO(id, "");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.Now} Невозможно вызвать GRPC Image сервер - {ex.Message}");
            return new ImageDTO(id, "");
        }
    }
}
