using System.Text.Json;
using Common.Contracts;
using FinanceService.Data;
using FinanceService.Entities;
using MassTransit;

namespace FinanceService.Consumers;

public class RestoreSnapShotDbConsumer : IConsumer<RestoreSnapShotItems<FinanceServiceType>>
{
    private readonly FinanceDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public RestoreSnapShotDbConsumer(FinanceDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<RestoreSnapShotItems<FinanceServiceType>> consumeContext)
    {
        var balanceCounter = 0;
        foreach (var item in consumeContext.Message.Items.OrderBy(p => p.RestoringOrder))
        {
            //восстанавливаем тип BalanceItem
            foreach (var items in item.Items)
            {
                var balanceItem = JsonSerializer.Deserialize<BalanceItem>(items);
                await _context.BalanceItems.AddAsync(balanceItem);
                balanceCounter++;
            }
        }
        await _context.SaveChangesAsync();
        await _publishEndpoint.Publish(new RestoreSnapShotCompleted($"Успешно восстановлено {balanceCounter} записей в balanceCounter",
            Guid.NewGuid(), consumeContext.Message.UserLogin, consumeContext.Message.SessionId));
        Console.WriteLine($"{DateTime.Now} --> Восстановлено {balanceCounter} записей в balanceCounter");
    }
}

