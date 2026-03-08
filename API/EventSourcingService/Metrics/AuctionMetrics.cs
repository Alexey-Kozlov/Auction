using System.Diagnostics.Metrics;

namespace AuctionService.Metrics;

public class AuctionMetrics
{
    private Counter<int> AuctionAddCounter { get; }
    private Counter<int> AuctionUpdateCounter { get; }
    private Counter<int> AuctionDeleteCounter { get; }
    private Counter<int> AuctionFinishedCounter { get; }
    private Counter<int> AuctionBidCounter { get; }
    private Counter<int> AuctionNotificationCounter { get; }
    private Counter<int> AuctionFinanceCounter { get; }
    private Counter<int> CommunicationAddCounter { get; }
    private Counter<int> CommunicationUpdateCounter { get; }
    private Counter<int> CommunicationDeleteCounter { get; }
    private Counter<int> TagCreateCounter { get; }
    private Counter<int> TagDeleteCounter { get; }

    public AuctionMetrics(IMeterFactory meterFactory, IConfiguration configuration)
    {
        var meter = meterFactory.Create(configuration["MetricConfig:MetricCustom:MetricGroup"]);
        AuctionAddCounter = meter.CreateCounter<int>(configuration["MetricConfig:MetricCustom:MetricNameAdd"], "Auction");
        AuctionDeleteCounter = meter.CreateCounter<int>(configuration["MetricConfig:MetricCustom:MetricNameDelete"], "Auction");
        AuctionUpdateCounter = meter.CreateCounter<int>(configuration["MetricConfig:MetricCustom:MetricNameUpdate"], "Auction");
        AuctionFinishedCounter = meter.CreateCounter<int>(configuration["MetricConfig:MetricCustom:MetricNameFinish"], "Auction");
        AuctionBidCounter = meter.CreateCounter<int>(configuration["MetricConfig:MetricCustom:MetricNameBid"], "Auction");
        AuctionNotificationCounter = meter.CreateCounter<int>(configuration["MetricConfig:MetricCustom:MetricNotification"], "Auction");
        AuctionFinanceCounter = meter.CreateCounter<int>(configuration["MetricConfig:MetricCustom:MetricFinance"], "Auction");
        CommunicationAddCounter = meter.CreateCounter<int>(configuration["MetricConfig:MetricCustom:MetricCommunicationAdd"], "Auction");
        CommunicationUpdateCounter = meter.CreateCounter<int>(configuration["MetricConfig:MetricCustom:MetricCommunicationUpdate"], "Auction");
        CommunicationDeleteCounter = meter.CreateCounter<int>(configuration["MetricConfig:MetricCustom:MetricCommunicationDelete"], "Auction");
        TagCreateCounter = meter.CreateCounter<int>(configuration["MetricConfig:MetricCustom:MetricTagCreate"], "Auction");
        TagDeleteCounter = meter.CreateCounter<int>(configuration["MetricConfig:MetricCustom:MetricTagDelete"], "Auction");
    }

    public void AddAuction() => AuctionAddCounter.Add(1);
    public void DeleteAuction() => AuctionDeleteCounter.Add(1);
    public void UpdateAuction() => AuctionUpdateCounter.Add(1);
    public void FinishAuction() => AuctionFinishedCounter.Add(1);
    public void BidAuction() => AuctionBidCounter.Add(1);
    public void NotificationAuction() => AuctionNotificationCounter.Add(1);
    public void FinanceAuction() => AuctionFinanceCounter.Add(1);
    public void AddCommunication() => CommunicationAddCounter.Add(1);
    public void UpdateCommunication() => CommunicationUpdateCounter.Add(1);
    public void DeleteCommunication() => CommunicationDeleteCounter.Add(1);
    public void CreateTag() => TagCreateCounter.Add(1);
    public void DeleteTag() => TagDeleteCounter.Add(1);

}