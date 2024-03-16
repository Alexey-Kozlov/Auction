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

    public AuctionCreatedConsumer(BidDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task Consume(ConsumeContext<AuctionCreated> consumeContext)
    {
        var auction = _mapper.Map<Auction>(consumeContext.Message);
        _context.Auctions.Add(auction);
        await _context.SaveChangesAsync();
    }
}
