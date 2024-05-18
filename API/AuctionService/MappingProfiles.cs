using AuctionService.DTO;
using AuctionService.Entities;
using AutoMapper;
using Contracts;

namespace AuctionService;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Auction, AuctionDTO>().IncludeMembers(p => p.Item);
        CreateMap<ICollection<Item>, AuctionDTO>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.FirstOrDefault().Title))
            .ForMember(dest => dest.Properties, opt => opt.MapFrom(src => src.FirstOrDefault().Properties))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.FirstOrDefault().Description));

        CreateMap<AuctionCreating, Auction>()
        .ForMember(dest => dest.Seller, opt => opt.MapFrom(src => src.AuctionAuthor))
        .ForMember(dest => dest.Item,
            dest => dest.MapFrom(src => new List<Item>{
                new() {
                    Title = src.Title,
                    Properties = src.Properties ?? "",
                    Description = src.Description ?? ""
                }
            }));

        CreateMap<AuctionUpdating, Auction>().ForMember(dest => dest.Item,
            opt => opt.MapFrom((src, dest) =>
            {
                return new List<Item>
                {
                new() {
                    Title = src.Title ?? dest.Item.First().Title,
                    Properties = src.Properties ?? dest.Item.First().Properties,
                    Description = src.Description ?? dest.Item.First().Description
                }
                };
            }));

        CreateMap<AuctionUpdating, AuctionUpdated>();
    }
}
