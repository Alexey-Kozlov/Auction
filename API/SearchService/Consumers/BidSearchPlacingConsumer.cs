using Common.Contracts;
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
        Console.WriteLine($"{DateTime.Now} Получение сообщения разместить заявку, автор - {consumerContext.Message.Bidder}");
        using var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
        try
        {
            //throw new Exception("");  //для теста отработки компенсирующих транзакций

            var auction = await _context.Items.FirstOrDefaultAsync(p => p.Id == consumerContext.Message.Id);
            if (consumerContext.Message.Amount <= auction.CurrentHighBid)
            {
                throw new Exception($"Ошибка обновления ставки в Search");
            }
            auction.CurrentHighBid = consumerContext.Message.Amount;
            await _context.SaveChangesAsync();
            await _publishEndpoint.Publish(new BidSearchPlaced(consumerContext.Message.CorrelationId));
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw new Exception($"Ошибка обновления ставки в Search - {e.Message}");
        }
    }
}
