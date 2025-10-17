using AutoMapper;
using Common.Contracts.Auction;
using Common.Contracts.Communication;

namespace ElasticSearchService;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<AuctionItem, AuctionCreatingElk>()
        .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.SoldAmount))
        .ForMember(dest => dest.UserLogin, opt => opt.MapFrom(src => src.Seller))
        .ForMember(dest => dest.AuctionCreated, opt => opt.MapFrom(src => src.CreateAt))
        .ForMember(dest => dest.ItemSold, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Winner)));

        CreateMap<CommunicationItem, CommunicationSearch>();

        CreateMap<AuctionCreatingElk, AuctionItem>()
        .ForMember(dest => dest.SoldAmount, opt => opt.MapFrom(src => src.Amount))
        .ForMember(dest => dest.Seller, opt => opt.MapFrom(src => src.UserLogin))
        .ForMember(dest => dest.CreateAt, opt => opt.MapFrom(src => src.AuctionCreated))
        .ForMember(dest => dest.Winner, opt => opt.MapFrom(src => src.ItemSold ? src.Winner : ""));

    }
}
