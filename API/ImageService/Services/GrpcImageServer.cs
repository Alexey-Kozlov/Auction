using Grpc.Core;
using ImageService;
using ImageService.Data;

namespace AuctionService.Services;

public class GrpcImageServer : GrpcImage.GrpcImageBase
{
    private readonly ImageDbContext _dbContext;

    public GrpcImageServer(ImageDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<GrpcImageResponse> GetImage(GetImageRequest request, ServerCallContext context)
    {
        Console.WriteLine("==> Получен запрос Grpc для получения изображения");
        var image = await _dbContext.Images.FindAsync(Guid.Parse(request.Id));

        if (image == null) throw new RpcException(new Status(StatusCode.NotFound, "Изображение не найдено"));

        var response = new GrpcImageResponse
        {
            Image = new GrpcImageModel
            {
                Id = image.Id.ToString(),
                Image = Convert.ToBase64String(image.Image)
            }
        };
        return response;
    }
}
