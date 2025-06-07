using Common.Contracts.Communication;
using Common.Contracts.Processing;

namespace ElasticSearchService.DTO;

public class CacheCommunicationDTO
{
    public CommunicationSearch Record { get; set; }
    public CRUD CRUD { get; set; }
}
