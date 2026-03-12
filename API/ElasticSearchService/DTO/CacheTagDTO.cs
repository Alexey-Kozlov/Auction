using Common.Contracts.Processing;
using Common.Contracts.Tag;

namespace ElasticSearchService.DTO;

public class CacheTagDTO
{
    public TagSearch Record { get; set; }
    public CRUD CRUD { get; set; }
}
