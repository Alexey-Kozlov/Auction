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

        var response = new GrpcImageResponse
        {
            Image = new GrpcImageModel
            {
                AuctionId = image == null ? "" : image.AuctionId.ToString(),
                Image = image == null ? "" : Convert.ToBase64String(image.Image)
            }
        };
        return response;
    }
}
