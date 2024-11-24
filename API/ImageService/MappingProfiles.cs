using AutoMapper;
using Common.Contracts.Auction;
using ImageService.Controllers;
using ImageService.Entities;

namespace ImageService;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {

        CreateMap<ImageItem, ImageDTO>()
            .ForMember(dest => dest.AuctionId, opt => opt.MapFrom(src => src.AuctionId.ToString()))
            .ForMember(dest => dest.Image, opt => opt.MapFrom((src, dest) =>
            {
                if (src.Image != null && src.Image.Length > 0)
                {
                    dest.Image = Convert.ToBase64String(src.Image);
                }
                return dest?.Image;
            })
        );

        CreateMap<ImageItem, ImageItem>();
    }
}
