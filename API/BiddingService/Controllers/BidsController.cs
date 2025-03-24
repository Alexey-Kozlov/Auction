using System.Linq.Expressions;
using AutoMapper;
using BiddingService.Data;
using BiddingService.DTO;
using Common.Contracts;
using Common.Contracts.Bid;
using Common.Contracts.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serialize.Linq.Serializers;

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
        var highBid = await _context.Bids.Where(p => p.Commited && p.AuctionId == Guid.Parse(auctionId))
                .OrderBy(p => p.BidTime).ToListAsync();

        return new ApiResponse<List<BidDTO>>()
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = highBid.Select(_mapper.Map<BidDTO>).ToList()
        };
    }

    [HttpPost("GetBidItemsByQuery")]
    public async Task<string> GetAuctionItemsByQuery(ReportParamsDTO dto)
    {
        var serializer = new ExpressionSerializer(new JsonSerializer())
        {
            AutoAddKnownTypesAsListTypes = true
        };
        var expression = serializer.DeserializeText(dto.Expression) as Expression<Func<BidItem, bool>>;
        var items = await _context.Bids.Where(expression).ToListAsync();
        return System.Text.Json.JsonSerializer.Serialize(new ApiResponse<List<BidItem>>()
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = items
        });

    }
}
