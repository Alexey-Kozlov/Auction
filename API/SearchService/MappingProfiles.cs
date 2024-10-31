using AutoMapper;
using Common.Contracts;

namespace SearchService;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<AuctionCreatingSearch, AuctionItem>()
        .ForMember(dest => dest.Seller, opt => opt.MapFrom(src => src.UserLogin))
            .ForMember(dest => dest.CreateAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.AuctionId, opt => opt.MapFrom(src => src.AuctionId));

        CreateMap<AuctionUpdatingSearch, AuctionItem>()
        .ForMember(dest => dest.Seller, opt => opt.MapFrom(src => src.UserLogin));

        CreateMap<AuctionItem, AuctionItem>()
        .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
        .ForMember(dest => dest.Properties, opt => opt.MapFrom(src => src.Properties))
        .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
        // .ForMember(dest => dest.ReservePrice, opt => opt.Ignore())
        // .ForMember(dest => dest.Seller, opt => opt.Ignore())
        // .ForMember(dest => dest.Winner, opt => opt.Ignore())
        // .ForMember(dest => dest.SoldAmount, opt => opt.Ignore())
        // .ForMember(dest => dest.CurrentHighBid, opt => opt.Ignore())
        // .ForMember(dest => dest.CreateAt, opt => opt.Ignore())
        .ForMember(dest => dest.CreateAt, opt => opt.MapFrom(src => DateTime.UtcNow))
        .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
        .ForMember(dest => dest.AuctionEnd, opt => opt.MapFrom(src => src.AuctionEnd));

        CreateMap<AuctionItem, AuctionCreatingElk>()
        .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.SoldAmount))
        .ForMember(dest => dest.UserLogin, opt => opt.MapFrom(src => src.Seller))
        .ForMember(dest => dest.AuctionCreated, opt => opt.MapFrom(src => src.CreateAt))
        .ForMember(dest => dest.ItemSold, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Winner)));

    }
}
