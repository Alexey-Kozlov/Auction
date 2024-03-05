using System.Security.Claims;
using AutoMapper;
using BiddingService.DTO;
using BiddingService.Models;
using BiddingService.Services;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;

namespace BiddingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BidsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly GrpcAuctionClient _grpcClient;

    public BidsController(IMapper mapper, IPublishEndpoint publishEndpoint, 
    GrpcAuctionClient grpcClient)
    {
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
        _grpcClient = grpcClient;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<BidDTO>> PlaceBid([FromBody] PlaceBidDTO par)
    {
        var auction = await DB.Find<Auction>().OneAsync(par.auctionId);
        if (auction == null)
        {
            auction = _grpcClient.GetAuction(par.auctionId);
            if(auction == null) return BadRequest("Невозможно назначить заявку на этот аукцион - аукцион не найден!");
        }

        if (auction.Seller == User.Identity.Name)
        {
            return BadRequest("Невозможно подать предложение для собственного аукциона");
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
            var highBid = await DB.Find<Bid>()
                .Match(p => p.AuctionId == par.auctionId)
                .Sort(p => p.Descending(x => x.Amount))
                .ExecuteFirstAsync();

            if (highBid != null && par.amount > highBid.Amount || highBid == null)
            {
                bid.BidStatus = par.amount > auction.ReservePrice ?
                BidStatus.Принято :
                BidStatus.ПринятоНижеНачальнойСтавки;
            }

            if (highBid != null && bid.Amount <= highBid.Amount)
            {
                bid.BidStatus = BidStatus.СлишкомНизкая;
            }
        }

        await DB.SaveAsync(bid);

        await _publishEndpoint.Publish(_mapper.Map<BidPlaced>(bid));

        return Ok(_mapper.Map<BidDTO>(bid));
    }

    [HttpGet("{auctionId}")]
    public async Task<ActionResult<List<BidDTO>>> GetBidsForAuction(string auctionId)
    {
        var bids = await DB.Find<Bid>()
            .Match(p => p.AuctionId == auctionId)
            .Sort(p => p.Ascending(x => x.BidTime))
            .ExecuteAsync();

        return bids.Select(_mapper.Map<BidDTO>).ToList();
    }
}