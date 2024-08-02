using Common.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;

namespace SearchService.Consumers;

public class AuctionDeletingSearchConsumer : IConsumer<AuctionDeletingSearch>
{
    private readonly SearchDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionDeletingSearchConsumer(SearchDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<AuctionDeletingSearch> context)
    {
        Console.WriteLine("--> Получение сообщения удалить аукцион");
        var item = await _context.Items.FirstOrDefaultAsync(p => p.Id == context.Message.Id);
        if (item != null)
        {
            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
        }
        await _publishEndpoint.Publish(new AuctionDeletedSearch(context.Message.CorrelationId));
    }
}
