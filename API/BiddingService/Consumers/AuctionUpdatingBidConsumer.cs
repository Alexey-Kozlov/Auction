using BiddingService.Data;
using Common.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;


namespace BiddingService.Consumers;

public class AuctionUpdatingBidConsumer : IConsumer<AuctionUpdatingBid>
{
    private readonly BidDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionUpdatingBidConsumer(BidDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<AuctionUpdatingBid> context)
    {
        var item = await _context.Auctions.FirstOrDefaultAsync(p => p.AuctionId == context.Message.AuctionId);
        if (item == null)
        {
            Console.WriteLine($"{DateTime.Now} Ошибка обновления записи - запись " + context.Message.AuctionId + " не найдена.");
            throw new Exception($"{DateTime.Now} Ошибка обновления записи - запись " + context.Message.AuctionId + " не найдена.");
        }
        item.AuctionEnd = context.Message.AuctionEnd;
        await _context.SaveChangesAsync();

        await _publishEndpoint.Publish(new AuctionUpdatedBid(context.Message.CorrelationId));

    }
}
