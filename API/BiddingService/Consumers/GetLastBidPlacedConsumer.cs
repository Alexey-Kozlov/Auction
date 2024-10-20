using BiddingService.Data;
using Common.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BiddingService.Consumers;

public class GetLastBidPlacedConsumer : IConsumer<GetLastBidPlaced>
{
    private readonly BidDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public GetLastBidPlacedConsumer(BidDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<GetLastBidPlaced> consumerContext)
    {
        Console.WriteLine($"{DateTime.Now} Получение сообщения - получить максимальную ставку по аукциону");
        var maxBid = await _context.Bids.Where(p => p.AuctionId == consumerContext.Message.Id)
            .MaxAsync(p => p.Amount);

        await _publishEndpoint.Publish(new GetCurrentBid(consumerContext.Message.CorrelationId, maxBid));
    }
}
