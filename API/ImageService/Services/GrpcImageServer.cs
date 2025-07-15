using Common.Contracts.Image;
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
        var itemId = Guid.NewGuid();
        ImageItem image = null;
        if (Guid.TryParse(request.ItemId, out itemId))
        {
            image = await _dbContext.Images.FirstOrDefaultAsync(p =>
                p.ItemId == Guid.Parse(request.ItemId) && p.Commited);
        }
        
        var response = new GrpcImageResponse
        {
            Image = new GrpcImageModel
            {
                ItemId = image == null ? "" : image.ItemId.ToString(),
                Image = image == null ? "" : Convert.ToBase64String(image.Image)
            }
        };
        return response;
    }
}
