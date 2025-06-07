using AutoMapper;
using Common.Contracts.Communication;
using CommunicationService.DTO;

namespace CommunicationService;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<CommunicationItem, CommunicationDTO>();
    }
}
