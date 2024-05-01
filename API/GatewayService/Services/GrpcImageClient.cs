using GatewayService.Models;
using Grpc.Core;
using Grpc.Net.Client;
using ImageService;

namespace GatewayService.Services;

public class GrpcImageClient
{
    private readonly ILogger<GrpcImageClient> _logger;
    private readonly IConfiguration _config;

    public GrpcImageClient(ILogger<GrpcImageClient> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public async Task<ImageDTO> GetImage(string id)
    {
        _logger.LogInformation("Вызов Grpc сервер");
        var channel = GrpcChannel.ForAddress(_config["GrpcImage"]);
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
                _logger.LogError("Изображение в БД не найдено");
                return new ImageDTO(id, "");
            }
            _logger.LogError(ex, "Ошибка GRPC Image");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Невозможно вызвать GRPC Image сервер");
            return null;
        }
    }
}
