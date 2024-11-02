using System.Text.Json;
using Common.Contracts;
using MassTransit;
using SearchService.Data;

namespace SearchService.Consumers;

public class RestoreSnapShotDbConsumer : IConsumer<RestoreSnapShotItems<SearchServiceType>>
{
    private readonly SearchDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public RestoreSnapShotDbConsumer(SearchDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<RestoreSnapShotItems<SearchServiceType>> consumeContext)
    {
        var itemCounter = 0;
        foreach (var item in consumeContext.Message.Items.OrderBy(p => p.RestoringOrder))
        {
            //восстанавливаем тип BalanceItem
            foreach (var items in item.Items)
            {
                var searchItem = JsonSerializer.Deserialize<AuctionItem>(items);
                await _context.AuctionItems.AddAsync(searchItem);
                itemCounter++;
            }
        }
        await _context.SaveChangesAsync();
        await _publishEndpoint.Publish(new RestoreSnapShotCompleted($"Успешно восстановлено {itemCounter} записей в SearchItems",
            Guid.NewGuid(), consumeContext.Message.UserLogin, consumeContext.Message.SessionId));
        Console.WriteLine($"{DateTime.Now} --> Восстановлено {itemCounter} записей в SearchItems");
    }
}

