using BiddingService.Data;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BiddingService.Consumers;

public class RollbackBidPlacedConsumer : IConsumer<RollbackBidPlaced>
{
    private readonly BidDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public RollbackBidPlacedConsumer(IPublishEndpoint publishEndpoint, BidDbContext dbContext)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<RollbackBidPlaced> context)
    {
        var bid = await _dbContext.Bids.Where(p => p.Id == context.Message.BidId).FirstOrDefaultAsync();
        _dbContext.Bids.Remove(bid);
        await _dbContext.SaveChangesAsync();
        Console.WriteLine($"{DateTime.Now} Получение сообщения - отмена размещения заявки - {context.Message.BidId}");
    }
}
