using AutoMapper;
using BiddingService.DTO;
using Common.Contracts;

namespace BiddingService;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<BidItem, BidDTO>();
        //CreateMap<AuctionItem, AuctionItem>();
    }
}
