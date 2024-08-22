using System.Diagnostics.Metrics;
using System.Collections.Generic;

namespace MetricsService.Metrics;

public class AuctionMetrics
{

    private Counter<int> AuctionAddCounter { get; }
    private Counter<int> AuctionUpdateCounter { get; }
    private Counter<int> AuctionDeleteCounter { get; }
    private Counter<int> AuctionBidUpCounter { get; }

    public AuctionMetrics(IMeterFactory meterFactory, IConfiguration configuration)
    {
        var meter = meterFactory.Create(configuration["AuctionStoreMeterName"] ??
            throw new NullReferenceException("AuctionStore meter отсутствует наименование"));
        AuctionAddCounter = meter.CreateCounter<int>("auction-added", "Auction");
        AuctionDeleteCounter = meter.CreateCounter<int>("auction-deleted", "Auction");
        AuctionUpdateCounter = meter.CreateCounter<int>("auction-updated", "Auction");
        AuctionBidUpCounter = meter.CreateCounter<int>("auction-bid-up", "Auction");
    }

    public void AddAuction() => AuctionAddCounter.Add(1);
    public void DeleteAuction() => AuctionDeleteCounter.Add(1);
    public void UpdateAuction() => AuctionUpdateCounter.Add(1);
    public void IncreaseAuctionBid() => AuctionBidUpCounter.Add(1);
    public void IncreaseAuctionBidder(string bidder) => AuctionBidUpCounter.Add(1,
        KeyValuePair.Create<string, object>("Bidder", bidder));

}