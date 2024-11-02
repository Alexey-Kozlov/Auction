using AutoMapper;
using BiddingService.DTO;
using Common.Contracts;

namespace BiddingService;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<BidItem, BidDTO>()
        .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AuctionId))
        .ForMember(dest => dest.BidId, opt => opt.MapFrom(src => src.BidId));
        // CreateMap<AuctionCreatingBid, AuctionItem>()
        //     .ForMember(dest => dest.Seller, opt => opt.MapFrom(src => src.UserLogin));
        // CreateMap<AuctionUpdatingBid, AuctionItem>();
        CreateMap<AuctionItem, AuctionItem>();
    }
}
