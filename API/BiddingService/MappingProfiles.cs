using AutoMapper;
using BiddingService.DTO;
using BiddingService.Entities;
using Contracts;

namespace BiddingService;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Bid, BidDTO>()
        .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AuctionId))
        .ForMember(dest => dest.BidId, opt => opt.MapFrom(src => src.Id));
        CreateMap<AuctionCreatingBid, Auction>()
            .ForMember(dest => dest.Seller, opt => opt.MapFrom(src => src.AuctionAuthor));
        CreateMap<AuctionUpdatingBid, Auction>();
        CreateMap<Auction, Auction>();
    }
}
