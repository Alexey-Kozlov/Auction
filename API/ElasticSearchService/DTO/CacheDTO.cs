using Common.Contracts.Auction;
using Common.Contracts.Processing;

namespace ElasticSearchService.DTO;

public class CacheDTO
{
    public AuctionCreatingElk Record { get; set; }
    public CRUD CRUD { get; set; }
}