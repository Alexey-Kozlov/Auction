using AuctionService.Data;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    private readonly AuctionDbContext _auctionDbContext;

    public BidPlacedConsumer(AuctionDbContext auctionDbContext)
    {
        _auctionDbContext = auctionDbContext;
    }
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        var auction = await _auctionDbContext.Auctions.FindAsync(Guid.Parse(context.Message.AuctionId.ToString()));
        if (context.Message.Amount > auction.CurrentHighBid)
        {
            auction.CurrentHighBid = context.Message.Amount;
            await _auctionDbContext.SaveChangesAsync();
            Console.WriteLine("--> Получение сообщения - размещена заявка, AuctionId - " + context.Message.AuctionId + ", ставка - "
                + context.Message.Amount);
        }
    }
}
