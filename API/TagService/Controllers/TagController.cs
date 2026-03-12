using Common.Contracts;
using Common.Contracts.Tag;
using Microsoft.AspNetCore.Mvc;
using TagService.Services;

namespace SettingsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagController : ControllerBase
{
    private readonly TagDataService _tagDataService;

    public TagController(TagDataService tagDataService)
    {
        _tagDataService = tagDataService;
    }


    [HttpGet("GetTagList")]
    public async Task<ApiResponse<List<TagList>>> GetTagList()
    {
        //возвращаем список тегов
        return await _tagDataService.GetTagList();
    }

    [HttpGet("GetAuctionTags/{auctionid}")]
    public async Task<ApiResponse<List<TagList>>> GetAuctionTags(Guid auctionid)
    {
        //возвращаем список тегов
        return await _tagDataService.GetAuctionTags(auctionid);
    }
}