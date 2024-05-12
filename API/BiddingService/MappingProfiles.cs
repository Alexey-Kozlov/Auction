using AutoMapper;
using BiddingService.DTO;
using BiddingService.Entities;
using Contracts;

namespace BiddingService;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Bid, BidDTO>();
        CreateMap<AuctionCreatingBid, Auction>()
            .ForMember(dest => dest.Seller, opt => opt.MapFrom(src => src.AuctionAuthor));
        CreateMap<AuctionUpdatingBid, Auction>();
        CreateMap<Auction, Auction>();
    }
}
