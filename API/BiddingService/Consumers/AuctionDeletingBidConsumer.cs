using Common.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using BiddingService.Data;

namespace BiddingService.Consumers;

public class AuctionDeletingBidConsumer : IConsumer<AuctionDeletingBid>
{
    private readonly BidDbContext _context;
    private readonly ILogger<AuctionDeletingBidConsumer> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionDeletingBidConsumer(BidDbContext context, ILogger<AuctionDeletingBidConsumer> logger, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<AuctionDeletingBid> context)
    {
        var bids = await _context.Bids.Where(p => p.AuctionId == context.Message.AuctionId).ToListAsync();
        if (bids != null && bids.Count > 0)
        {
            _context.Bids.RemoveRange(bids);
        }
        var auction = await _context.Auctions.FirstOrDefaultAsync(p => p.AuctionId == context.Message.AuctionId);
        if (auction != null)
        {
            _context.Auctions.Remove(auction);
        }
        await _context.SaveChangesAsync();

        if (bids.Count > 0)
        {
            _logger.LogInformation("Удалены " + bids.Count.ToString() + " ставок для этого аукциона");
        }
        await _publishEndpoint.Publish(new AuctionDeletedBid(context.Message.CorrelationId));
    }
}
