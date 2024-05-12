
using BiddingService.Data;
using BiddingService.Entities;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

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
        stoppingToken.Register(() => _logger.LogInformation("Остановлен сервис по проверке завершения апукционов/"));
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckAuction(stoppingToken);
            await Task.Delay(10000, stoppingToken);
        }
    }

    private async Task CheckAuction(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _services.CreateScope();
            var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            var _context = scope.ServiceProvider.GetRequiredService<BidDbContext>();

            var finishedAuctions = await _context.Auctions.Where(p => p.AuctionEnd <= DateTime.UtcNow && !p.Finished).ToListAsync();
            if (finishedAuctions.Count == 0) return;

            _logger.LogInformation("==> Найдено {count} аукционов, которые завершились", finishedAuctions.Count);

            foreach (var auction in finishedAuctions)
            {
                auction.Finished = true;
                await _context.SaveChangesAsync();
                var winningBid = await _context.Bids.Where(p =>
                p.AuctionId == auction.Id &&
                p.BidStatus == BidStatus.Принято)
                .OrderByDescending(p => p.Amount)
                .ThenBy(p => p.BidTime).FirstOrDefaultAsync();

                // await endpoint.Publish(new RequestAuctionUpdate(winningBid != null, auction.Id, winningBid?.Bidder,
                //     auction.Seller, (winningBid == null ? 0 : winningBid.Amount), new Guid()), stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка проверки аукционов на завершение");
        }

    }

}
