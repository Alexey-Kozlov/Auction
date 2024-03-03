
using BiddingService.Models;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace BiddingService.Services;

public class CheckAuctionFinished : BackgroundService
{
    private readonly ILogger<CheckAuctionFinished> _logger;
    private readonly IServiceProvider _services;

    public CheckAuctionFinished(ILogger<CheckAuctionFinished> logger, IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Старт сервиса по проверке завершения аукционов...");
        stoppingToken.Register(()=> _logger.LogInformation("Остановлен сервис по проверке завершения апукционов/"));
        while(!stoppingToken.IsCancellationRequested)
        {
            await CheckAuction(stoppingToken);
            await Task.Delay(10000, stoppingToken);
        }
    }

    private async Task CheckAuction(CancellationToken stoppingToken)
    {
        try
        {
            var finishedAuctions = await DB.Find<Auction>()
            .Match(p => p.AuctionEnd <= DateTime.UtcNow)
            .Match(p => !p.Finished)
            .ExecuteAsync(stoppingToken);
        
        if(finishedAuctions.Count == 0) return;

        _logger.LogInformation("==> Найдено {count} аукционов, которые завершились", finishedAuctions.Count);

        using var scope = _services.CreateScope();
        var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        foreach(var auction in finishedAuctions)
        {
            auction.Finished = true;
            await auction.SaveAsync(null, stoppingToken);

            var winningBid = await DB.Find<Bid>()
                .Match(p => p.AuctionId == auction.ID)
                .Match(p => p.BidStatus == BidStatus.Принято)
                .Sort(p => p.Descending(x => x.Amount))
                .ExecuteFirstAsync(stoppingToken);

            await endpoint.Publish(new AuctionFinished{
                ItemSold = winningBid != null,
                AuctionId = auction.ID,
                Winner = winningBid?.Bidder,
                Amount = winningBid == null ? 0 : winningBid.Amount,
                Seller = auction.Seller
            }, stoppingToken);
        }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка проверки аукционов на завершение");
        }
        
    }

}
