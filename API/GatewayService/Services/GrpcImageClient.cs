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

    public async Task<ImageDTO> GetImage(string ItemId)
    {
        var channel = GrpcChannel.ForAddress(_config["GrpcImage"], new GrpcChannelOptions
        {
            MaxSendMessageSize = int.MaxValue,
            MaxReceiveMessageSize = int.MaxValue
        });
        var client = new GrpcImage.GrpcImageClient(channel);
        var request = new GetImageRequest { ItemId = ItemId };

        try
        {
            var reply = await client.GetImageAsync(request);
            var imageDto = new ImageDTO(ItemId, reply.Image.Image);
            return imageDto;
        }
        catch (RpcException ex)
        {
            if (ex.StatusCode == StatusCode.NotFound)
            {
                return new ImageDTO(ItemId, "");
            }
            Console.WriteLine($"{DateTime.Now} Ошибка GRPC Image - {ex.Message}");
            return new ImageDTO(ItemId, "");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.Now} Невозможно вызвать GRPC Image сервер - {ex.Message}");
            return new ImageDTO(ItemId, "");
        }
    }
}
