using Grpc.Core;
using ImageService.Data;
using Microsoft.EntityFrameworkCore;

namespace ImageService.Services;

public class GrpcImageServer : GrpcImage.GrpcImageBase
{
    private readonly ImageDbContext _dbContext;

    public GrpcImageServer(ImageDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task<GrpcImageResponse> GetImage(GetImageRequest request, ServerCallContext context)
    {
        var image = await _dbContext.Images.FirstOrDefaultAsync(p => p.AuctionId == Guid.Parse(request.AuctionId) && p.Commited);

        if (image == null) throw new RpcException(new Status(StatusCode.NotFound, "Изображение не найдено"));

        var response = new GrpcImageResponse
        {
            Image = new GrpcImageModel
            {
                AuctionId = image.AuctionId.ToString(),
                Image = Convert.ToBase64String(image.Image)
            }
        };
        return response;
    }
}
