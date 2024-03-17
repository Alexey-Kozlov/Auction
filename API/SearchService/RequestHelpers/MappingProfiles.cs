using AutoMapper;
using Contracts;
using SearchService.Entities;

namespace SearchService;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<AuctionCreated, Item>();
        CreateMap<AuctionUpdated, Item>();
        CreateMap<Item, Item>()
        .ForMember(dest => dest.Make, opt => opt.MapFrom(src => src.Make))
        .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Model))
        .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year))
        .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Color))
        .ForMember(dest => dest.Mileage, opt => opt.MapFrom(src => src.Mileage))
        .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
        .ForMember(dest => dest.Status, opt => opt.Ignore())
        .ForMember(dest => dest.ReservePrice, opt => opt.Ignore())
        .ForMember(dest => dest.Seller, opt => opt.Ignore())
        .ForMember(dest => dest.Winner, opt => opt.Ignore())
        .ForMember(dest => dest.SoldAmount, opt => opt.Ignore())
        .ForMember(dest => dest.CurrentHighBid, opt => opt.Ignore())
        .ForMember(dest => dest.CreateAt, opt => opt.Ignore())
        .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
        .ForMember(dest => dest.AuctionEnd, opt => opt.MapFrom(src => src.AuctionEnd));
    }
}
