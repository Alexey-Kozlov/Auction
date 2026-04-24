using BiddingService.DTO;
using BiddingService.Services;
using Common.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace BiddingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BidsController : ControllerBase
{
    private readonly GetBidsService _bidsService;
    public BidsController(GetBidsService bidsService)
    {
        _bidsService = bidsService;
    }

    //получение списка ставок для аукциона
    [HttpGet("{auctionId:guid}")]
    public async Task<ApiResponse<List<BidDTO>>> GetBidsForAuction([FromRoute] Guid auctionId)
    {
        return await _bidsService.GetBidsForAuction(auctionId);
    }
}
