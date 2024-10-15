using System.Diagnostics.Metrics;

namespace AuctionService.Metrics;

public class AuctionMetrics
{
    private Counter<int> AuctionAddCounter { get; }
    private Counter<int> AuctionUpdateCounter { get; }
    private Counter<int> AuctionDeleteCounter { get; }
    private Counter<int> AuctionFinishedCounter { get; }
    private Counter<int> AuctionBidCounter { get; }

    public AuctionMetrics(IMeterFactory meterFactory, IConfiguration configuration)
    {
        var meter = meterFactory.Create(configuration["MetricConfig:MetricCustom:MetricGroup"]);
        AuctionAddCounter = meter.CreateCounter<int>(configuration["MetricConfig:MetricCustom:MetricNameAdd"], "Auction");
        AuctionDeleteCounter = meter.CreateCounter<int>(configuration["MetricConfig:MetricCustom:MetricNameDelete"], "Auction");
        AuctionUpdateCounter = meter.CreateCounter<int>(configuration["MetricConfig:MetricCustom:MetricNameUpdate"], "Auction");
        AuctionFinishedCounter = meter.CreateCounter<int>(configuration["MetricConfig:MetricCustom:MetricNameFinish"], "Auction");
        AuctionBidCounter = meter.CreateCounter<int>(configuration["MetricConfig:MetricCustom:MetricNameBid"], "Auction");
    }

    public void AddAuction() => AuctionAddCounter.Add(1);
    public void DeleteAuction() => AuctionDeleteCounter.Add(1);
    public void UpdateAuction() => AuctionUpdateCounter.Add(1);
    public void FinishAuction() => AuctionFinishedCounter.Add(1);
    public void BidAuction() => AuctionBidCounter.Add(1);

}