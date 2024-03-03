using AutoMapper;
using BiddingService.DTO;
using BiddingService.Models;
using Contracts;

namespace BiddingService.RequestHelpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Bid, BidDTO>();
        CreateMap<Bid, BidPlaced>();
    }
}
