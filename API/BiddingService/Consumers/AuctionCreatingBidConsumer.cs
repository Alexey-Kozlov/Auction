using AutoMapper;
using BiddingService.Data;
using Common.Contracts;
using MassTransit;

namespace BiddingService.Consumers;

public class AuctionCreatingBidConsumer : IConsumer<AuctionCreatingBid>
{
    private readonly BidDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionCreatingBidConsumer(BidDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<AuctionCreatingBid> context)
    {
        var auction = _mapper.Map<AuctionBidItem>(context.Message);
        _context.Auctions.Add(auction);
        await _context.SaveChangesAsync();
        await _publishEndpoint.Publish(new AuctionCreatedBid(context.Message.CorrelationId));
    }
}
