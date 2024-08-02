using Common.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;

namespace SearchService.Consumers;

public class AuctionFinishingSearchConsumer : IConsumer<AuctionFinishingSearch>
{
    private readonly SearchDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionFinishingSearchConsumer(SearchDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<AuctionFinishingSearch> consumeContext)
    {
        var auction = await _context.Items.FirstOrDefaultAsync(p => p.Id == consumeContext.Message.Id);
        if (auction != null)
        {
            if (consumeContext.Message.ItemSold)
            {
                auction.Winner = consumeContext.Message.Winner;
                auction.SoldAmount = consumeContext.Message.Amount;
            }
            await _context.SaveChangesAsync();
            await _publishEndpoint.Publish(new AuctionFinishedSearch(consumeContext.Message.CorrelationId));
            Console.WriteLine($"{DateTime.Now} --> Получение сообщения - аукцион завершен");
            return;
        }
        Console.WriteLine("Ошибка завершения аукциона " + auction.Id);
        throw new Exception("Ошибка завершения аукциона " + auction.Id + " - аукцион не найден");
    }
}
