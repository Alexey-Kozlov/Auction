using AutoMapper;
using Common.Contracts.Auction;

namespace SearchService;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        // CreateMap<AuctionCreatingSearch, AuctionItem>()
        // .ForMember(dest => dest.Seller, opt => opt.MapFrom(src => src.UserLogin))
        //     .ForMember(dest => dest.CreateAt, opt => opt.MapFrom(src => DateTime.UtcNow))
        //     .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
        //     .ForMember(dest => dest.AuctionId, opt => opt.MapFrom(src => src.AuctionId));

        // CreateMap<AuctionUpdatingSearch, AuctionItem>()
        // .ForMember(dest => dest.Seller, opt => opt.MapFrom(src => src.UserLogin));

        CreateMap<AuctionItem, AuctionItem>()
        .ForMember(dest => dest.CreateAt, opt => opt.MapFrom(src => DateTime.UtcNow))
        .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<AuctionItem, AuctionCreatingElk>()
        .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.SoldAmount))
        .ForMember(dest => dest.UserLogin, opt => opt.MapFrom(src => src.Seller))
        .ForMember(dest => dest.AuctionCreated, opt => opt.MapFrom(src => src.CreateAt))
        .ForMember(dest => dest.ItemSold, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Winner)));

    }
}
