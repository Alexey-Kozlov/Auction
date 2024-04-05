using System.Security.Claims;
using AutoMapper;
using BiddingService.Data;
using BiddingService.DTO;
using BiddingService.Models;
using BiddingService.Services;
using Common.Utils;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BiddingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BidsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly GrpcAuctionClient _grpcClient;
    private readonly BidDbContext _context;

    public BidsController(IMapper mapper, IPublishEndpoint publishEndpoint,
    GrpcAuctionClient grpcClient, BidDbContext context)
    {
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
        _grpcClient = grpcClient;
        _context = context;
    }

    [Authorize]
    [HttpPost]
    public async Task<ApiResponse<BidDTO>> PlaceBid([FromBody] PlaceBidDTO par)
    {
        var auction = await _context.Auctions.FirstOrDefaultAsync(p => p.Id == par.auctionId);
        if (auction == null)
        {
            auction = _grpcClient.GetAuction(par.auctionId.ToString());
            if (auction == null) return new ApiResponse<BidDTO>()
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                IsSuccess = false,
                ErrorMessages = ["Невозможно назначить заявку на этот аукцион - аукцион не найден!"],
                Result = null
            };
        }

        if (auction.Seller == ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login").Select(p => p.Value).FirstOrDefault())
        {
            return new ApiResponse<BidDTO>()
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                IsSuccess = false,
                ErrorMessages = ["Невозможно подать предложение для собственного аукциона"],
                Result = null
            };
        }

        var bid = new Bid
        {
            Amount = par.amount,
            AuctionId = par.auctionId,
            Bidder = User.Identity.Name
        };

        if (auction.AuctionEnd < DateTime.UtcNow)
        {
            bid.BidStatus = BidStatus.Завершено;
        }
        else
        {
            var highBid = await _context.Bids.Where(p => p.AuctionId == par.auctionId)
                .OrderByDescending(p => p.Amount).FirstOrDefaultAsync();

            if (highBid != null && par.amount > highBid.Amount || highBid == null)
            {
                bid.BidStatus = BidStatus.Принято;
            }
        }
        await _context.Bids.AddAsync(bid);
        await _context.SaveChangesAsync();

        await _publishEndpoint.Publish(_mapper.Map<BidPlaced>(bid));

        return new ApiResponse<BidDTO>()
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = _mapper.Map<BidDTO>(bid)
        };
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