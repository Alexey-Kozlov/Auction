using AutoMapper;
using BiddingService.Data;
using BiddingService.DTO;
using Common.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BiddingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BidsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly BidDbContext _context;

    public BidsController(IMapper mapper, BidDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }


    [HttpGet("{auctionId}")]
    public async Task<ApiResponse<List<BidDTO>>> GetBidsForAuction(string auctionId)
    {
        var highBid = await _context.Bids.Where(p => p.AuctionId == Guid.Parse(auctionId))
                .OrderBy(p => p.BidTime).ToListAsync();

        return new ApiResponse<List<BidDTO>>()
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = highBid.Select(_mapper.Map<BidDTO>).ToList()
        };
    }
}