using AutoMapper;
using Contracts;
using ImageService.Controllers;
using ImageService.Entities;

namespace ImageService;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<AuctionCreated, ImageItem>()
            .ForMember(dest => dest.AuctionId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Image, opt => opt.MapFrom((src, dest) =>
            {
                if (src.Image != null && src.Image.Length > 0)
                {
                    dest.Image = Convert.FromBase64String(src.Image
                         .Replace("data:image/png;base64,", "")
                         .Replace("data:image/jpeg;base64,", "")
                         .Replace("data:image/jpg;base64,", ""));
                }
                return dest?.Image;
            })
        );

        CreateMap<AuctionUpdatingImage, ImageItem>()
            .ForMember(dest => dest.AuctionId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Image, opt => opt.MapFrom((src, dest) =>
            {
                if (src.Image != null && src.Image.Length > 0)
                {
                    dest.Image = Convert.FromBase64String(src.Image
                         .Replace("data:image/png;base64,", "")
                         .Replace("data:image/jpeg;base64,", "")
                         .Replace("data:image/jpg;base64,", ""));
                }
                return dest?.Image;
            })
        );

        CreateMap<ImageItem, ImageDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
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
