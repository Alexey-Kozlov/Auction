using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;

namespace ProcessingService.Data;

public class ProcessingDbContext : SagaDbContext
{
    public ProcessingDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override IEnumerable<ISagaClassMap> Configurations
    {
        get
        {
            yield return new CreateBidStateMap();
            yield return new UpdateAuctionStateMap();
            yield return new DeleteAuctionStateMap();
            yield return new CreateAuctionStateMap();
        }
    }
}

