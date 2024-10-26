using System.Text.Json;
using Common.Contracts;
using MassTransit;
using NotificationService.Data;
using NotificationService.Entities;

namespace NotificationService.Consumers;

public class RestoreSnapShotDbConsumer : IConsumer<RestoreSnapShotItems<NotificationServiceType>>
{
    private readonly NotificationDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public RestoreSnapShotDbConsumer(NotificationDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<RestoreSnapShotItems<NotificationServiceType>> consumeContext)
    {
        var notifyCounter = 0;
        foreach (var item in consumeContext.Message.Items.OrderBy(p => p.RestoringOrder))
        {
            //восстанавливаем тип BalanceItem
            foreach (var items in item.Items)
            {
                var notifyItem = JsonSerializer.Deserialize<NotifyUser>(items);
                await _context.NotifyUser.AddAsync(notifyItem);
                notifyCounter++;
            }
        }
        await _context.SaveChangesAsync();
        await _publishEndpoint.Publish(new RestoreSnapShotCompleted($"Успешно восстановлено {notifyCounter} записей в NotifyUser",
            Guid.NewGuid(), consumeContext.Message.UserLogin, consumeContext.Message.SessionId));
        Console.WriteLine($"{DateTime.Now} --> Восстановлено {notifyCounter} записей в NotifyUser");
    }
}

