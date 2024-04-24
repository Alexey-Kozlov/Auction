using AutoMapper;
using BiddingService.DTO;
using BiddingService.Models;
using Contracts;

namespace BiddingService;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Bid, BidDTO>();
        CreateMap<AuctionCreated, Auction>();
    }
}
