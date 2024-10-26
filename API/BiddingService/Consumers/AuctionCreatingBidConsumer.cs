using System.Reflection;
using AutoMapper;
using BiddingService.Data;
using BiddingService.Entities;
using Common.Contracts;
using Common.Utils;
using MassTransit;

namespace BiddingService.Consumers;

public class AuctionCreatingBidConsumer : IConsumer<AuctionCreatingBid>
{
    private readonly BidDbContext _context;
    private readonly IMapper _mapper;

    public AuctionCreatingBidConsumer(BidDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task Consume(ConsumeContext<AuctionCreatingBid> context)
    {
        var auction = _mapper.Map<Auction>(context.Message);
        _context.Auctions.Add(auction);
        await _context.SaveChangesAsync();

    }
}
