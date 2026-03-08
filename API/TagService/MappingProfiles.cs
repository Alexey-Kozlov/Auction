using AutoMapper;
using Common.Contracts.Tag;

namespace SearchService;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<ModifyTag, TagItem>();

    }
}
