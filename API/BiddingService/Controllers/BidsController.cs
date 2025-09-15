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


    [HttpGet("{auctionId}")]
    public async Task<ApiResponse<List<BidDTO>>> GetBidsForAuction(string auctionId)
    {
        return await _bidsService.GetBidsForAuction(auctionId);
    }
}
