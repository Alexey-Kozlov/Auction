using Common.Contracts;
using Common.Contracts.Tag;
using Microsoft.EntityFrameworkCore;
using TagService.Data;

namespace TagService.Services;

public class TagDataService
{
    private readonly TagDbContext _dbContext;

    public TagDataService(TagDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<List<TagList>>> GetTagList()
    {
        var tagList = new List<TagList>();
        tagList.AddRange(await _dbContext.TagLists.ToListAsync());
        return new ApiResponse<List<TagList>>
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = tagList
        };
    }

    public async Task<ApiResponse<List<TagItem>>> GetAuctionTags(Guid auctionId)
    {
        var auctionTagList = new List<TagItem>();
        auctionTagList.AddRange(await _dbContext.TagItems.Where(p => p.AuctionId == auctionId).ToListAsync());
        return new ApiResponse<List<TagItem>>
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = auctionTagList
        };
    }
}
