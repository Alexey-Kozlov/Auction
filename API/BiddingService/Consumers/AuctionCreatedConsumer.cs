using AutoMapper;
using BiddingService.Data;
using BiddingService.Models;
using Contracts;
using MassTransit;

namespace BiddingService.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    private readonly BidDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AuctionCreatedConsumer> _logger;

    public AuctionCreatedConsumer(BidDbContext context, IMapper mapper, ILogger<AuctionCreatedConsumer> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<AuctionCreated> consumeContext)
    {
        var auction = _mapper.Map<Auction>(consumeContext.Message);
        _context.Auctions.Add(auction);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Создан аукцион - " + auction.Id + ", продавец - " + auction.Seller);
    }
}
