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

        CreateMap<CreateAuctionDTO, Auction>().ForMember(dest => dest.Item,
            dest => dest.MapFrom(src => new List<Item>{
                new() {
                    Title = src.Title,
                    Properties = src.Properties,
                    Description = src.Description
                }
            }));

        CreateMap<AuctionDTO, AuctionCreated>();
        CreateMap<Auction, AuctionUpdated>().IncludeMembers(p => p.Item);
        CreateMap<ICollection<Item>, AuctionUpdated>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.First().Title))
            .ForMember(dest => dest.Properties, opt => opt.MapFrom(src => src.First().Properties))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.First().Description));

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
