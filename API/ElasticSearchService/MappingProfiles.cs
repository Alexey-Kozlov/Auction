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

    }
}
