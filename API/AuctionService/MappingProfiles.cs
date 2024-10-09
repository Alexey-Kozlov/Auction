using AuctionService.Commands;
using AuctionService.DTO;
using AuctionService.Entities;
using AutoMapper;
using Common.Contracts;

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

        CreateMap<UpdateAuctionCommand, Auction>().ForMember(dest => dest.Item,
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

        CreateMap<CreateAuctionDTO, CreateAuctionCommand>()
        .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
        .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
        .ForMember(dest => dest.Properties, opt => opt.MapFrom(src => src.Properties))
        .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
        .ForMember(dest => dest.AuctionEnd, opt => opt.MapFrom(src => src.AuctionEnd))
         .ForMember(dest => dest.ReservePrice, opt => opt.MapFrom(src => src.ReservePrice))
        .ForMember(dest => dest.Type, opt => opt.MapFrom((src, dest) => dest.GetType().ToString()));

        CreateMap<UpdateAuctionDTO, UpdateAuctionCommand>()
        .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
        .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
        .ForMember(dest => dest.Properties, opt => opt.MapFrom(src => src.Properties))
        .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
        .ForMember(dest => dest.AuctionEnd, opt => opt.MapFrom(src => src.AuctionEnd))
         .ForMember(dest => dest.ReservePrice, opt => opt.MapFrom(src => src.ReservePrice))
        .ForMember(dest => dest.Type, opt => opt.MapFrom((src, dest) => dest.GetType().ToString()));

        CreateMap<Auction, TransferAuctionCommand>()
        .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
        .ForMember(dest => dest.ReservePrice, opt => opt.MapFrom(src => src.ReservePrice))
        .ForMember(dest => dest.EditUser, opt => opt.MapFrom(src => src.Seller))
        .ForMember(dest => dest.AuctionEnd, opt => opt.MapFrom(src => src.AuctionEnd))
        .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Item.FirstOrDefault().Title))
        .ForMember(dest => dest.Properties, opt => opt.MapFrom(src => src.Item.FirstOrDefault().Properties))
        .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Item.FirstOrDefault().Description))
        .ForMember(dest => dest.Seller, opt => opt.MapFrom(src => src.Seller))
        .ForMember(dest => dest.Winner, opt => opt.MapFrom(src => src.Winner))
        .ForMember(dest => dest.SoldAmount, opt => opt.MapFrom(src => src.SoldAmount))
        .ForMember(dest => dest.CurrentHighBid, opt => opt.MapFrom(src => src.CurrentHighBid))
        .ForMember(dest => dest.CreateAt, opt => opt.MapFrom(src => src.CreateAt))
        .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
        .ForMember(dest => dest.Type, opt => opt.MapFrom((src, dest) => dest.GetType().ToString()))
        .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

        CreateMap<AuctionUpdating, AuctionUpdated>();
    }
}
