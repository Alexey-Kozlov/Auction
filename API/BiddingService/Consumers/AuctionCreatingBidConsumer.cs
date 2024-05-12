using AutoMapper;
using BiddingService.Data;
using BiddingService.Entities;
using Contracts;
using MassTransit;

namespace BiddingService.Consumers;

public class AuctionCreatingBidConsumer : IConsumer<AuctionCreatingBid>
{
    private readonly BidDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AuctionCreatingBidConsumer> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionCreatingBidConsumer(BidDbContext context, IMapper mapper,
    ILogger<AuctionCreatingBidConsumer> logger, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<AuctionCreatingBid> consumeContext)
    {
        var auction = _mapper.Map<Auction>(consumeContext.Message);
        _context.Auctions.Add(auction);
        await _context.SaveChangesAsync();
        await _publishEndpoint.Publish(new AuctionCreatedBid(consumeContext.Message.CorrelationId));
        _logger.LogInformation("Создан аукцион - " + auction.Id + ", продавец - " + auction.Seller);
    }
}
