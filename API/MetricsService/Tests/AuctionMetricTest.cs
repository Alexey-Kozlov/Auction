using System.Diagnostics.Metrics;
using Microsoft.Extensions.Diagnostics.Metrics.Testing;
using Xunit;

namespace MetricsService.Tests;

public class AuctionMetricsTest
{
    private static IServiceProvider CreateServiceProvider()
    {
        var serviceCollection = new ServiceCollection();
        var config = CreateIConfiguration();
        serviceCollection.AddMetrics();
        serviceCollection.AddSingleton(config);
        serviceCollection.AddSingleton<Metrics.AuctionMetrics>();
        return serviceCollection.BuildServiceProvider();
    }

    private static IConfiguration CreateIConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string> {
                {"AuctionStoreMeterName", "AuctionStore"}
            };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();
    }


    [Fact]
    public void AuctionCreatingMetric()
    {
        var services = CreateServiceProvider();
        var metrics = services.GetRequiredService<Metrics.AuctionMetrics>();
        var meterFactory = services.GetRequiredService<IMeterFactory>();
        var collector = new MetricCollector<int>(meterFactory, "AuctionStore", "auction-added");

        metrics.AddAuction();
        metrics.AddAuction();
        metrics.AddAuction();
        var measurements = collector.GetMeasurementSnapshot();
        var ll = measurements[1].Value;
        var tt = measurements.EvaluateAsCounter();
    }

    [Fact]
    public void AuctionCreatingMetric2()
    {
        var services = CreateServiceProvider();
        var metrics = services.GetRequiredService<Metrics.AuctionMetrics>();
        var meterFactory = services.GetRequiredService<IMeterFactory>();
        var collector = new MetricCollector<int>(meterFactory, "AuctionStore", "auction-bid-up");

        metrics.IncreaseAuctionBidder("Alice");

        var measurements = collector.GetMeasurementSnapshot();
        var rez = measurements.ContainsTags("Bidder").FirstOrDefault()?.Tags.FirstOrDefault().Value;
    }
}