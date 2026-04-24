using AutoMapper;
using BiddingService.Data;
using BiddingService.DTO;
using Common.Contracts;
using Microsoft.EntityFrameworkCore;

namespace BiddingService.Services;

public class GetBidsService
{
    private readonly IMapper _mapper;
    private readonly BidDbContext _context;

    public GetBidsService(IMapper mapper, BidDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task<ApiResponse<List<BidDTO>>> GetBidsForAuction(Guid auctionId)
    {
        var highBid = await _context.Bids.Where(p => p.Commited && p.AuctionId == auctionId)
                .OrderBy(p => p.BidTime).ToListAsync();

        return new ApiResponse<List<BidDTO>>()
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = highBid.Select(_mapper.Map<BidDTO>).ToList()
        };
    }

}