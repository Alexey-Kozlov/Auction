using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;

namespace SearchService.Consumers;

public class BidSearchPlacingConsumer : IConsumer<BidSearchPlacing>
{
    private readonly SearchDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public BidSearchPlacingConsumer(SearchDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<BidSearchPlacing> consumerContext)
    {
        Console.WriteLine("--> Получение сообщения разместить заявку");

        //throw new Exception("Ошибка создания ставки");

        var auction = await _context.Items.FirstOrDefaultAsync(p => p.Id == consumerContext.Message.Id);
        if (consumerContext.Message.Amount > auction.CurrentHighBid)
        {
            auction.CurrentHighBid = consumerContext.Message.Amount;
            await _context.SaveChangesAsync();
        }
        await _publishEndpoint.Publish(new BidSearchPlaced(consumerContext.Message.CorrelationId));
    }
}
