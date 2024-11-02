using Common.Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class AuctionNotificationConsumer : IConsumer<ActionMessageList<NotifyItem>>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly NotificationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionNotificationConsumer(IHubContext<NotificationHub> hubContext,
    NotificationDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _hubContext = hubContext;
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<ActionMessageList<NotifyItem>> context)
    {
        var correlationId = context.Message.ActionItemsList[0].CorrelationId;
        foreach (var actionItem in context.Message.ActionItemsList)
        {
            switch (actionItem.OperationType)
            {
                //удаляем получателя сообщений аукциона
                case OperationType.Delete:
                    var delItem = await CheckExistItem(actionItem);
                    _dbContext.NotifyItems.Remove(delItem);
                    break;
                //подписываем на получение сообщений аукциона
                case OperationType.Insert:
                    _dbContext.NotifyItems.Add(actionItem.ActionItem);
                    await _dbContext.SaveChangesAsync();
                    await _publishEndpoint.Publish(new AuctionCreatedNotification(correlationId));
                    break;
            }
        }
        await _hubContext.Clients.All.SendAsync("AuctionCreated", context.Message);
    }
    private async Task<NotifyItem> CheckExistItem(ActionMessage<NotifyItem> notifyItem)
    {
        var item = await _dbContext.NotifyItems.FirstOrDefaultAsync(p => p.AuctionId == notifyItem.ActionItem.AuctionId &&
            p.UserLogin == notifyItem.ActionItem.UserLogin);
        if (item == null)
        {
            Console.WriteLine($"{DateTime.Now} Ошибка записи - запись " + notifyItem.ActionItem.AuctionId + " не найдена.");
            throw new Exception($"{DateTime.Now} Ошибка записи - запись " + notifyItem.ActionItem.AuctionId + " не найдена.");
        }
        return item;
    }
}
