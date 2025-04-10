using AutoMapper;
using Common.Contracts.Image;

namespace ImageService;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {

        CreateMap<ImageItem, ImageDTO>()
            .ForMember(dest => dest.Image, opt => opt.MapFrom((src, dest) =>
            {
                if (src.Image != null && src.Image.Length > 0)
                {
                    dest.Image = "data:image/jpg;base64," + Convert.ToBase64String(src.Image);
                }
                return dest?.Image;
            })
        );

        CreateMap<ImageDTO, ImageItem>()
            .ForMember(dest => dest.Image, opt => opt.MapFrom((src, dest) =>
            {
                if (src.Image != null && src.Image.Length > 0)
                {
                    dest.Image = Convert.FromBase64String(src.Image
                        .Replace("data:image/png;base64,", "")
                        .Replace("data:image/jpeg;base64,", "")
                        .Replace("data:image/bmp;base64,", "")
                        .Replace("data:image/jpg;base64,", ""));
                }
                return dest?.Image;
            }))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()));


        CreateMap<ImageItem, ImageItem>();
    }
}
