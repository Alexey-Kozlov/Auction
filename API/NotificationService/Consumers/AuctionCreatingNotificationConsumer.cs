using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Data;
using NotificationService.Entities;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class AuctionCreatingNotificationConsumer : IConsumer<AuctionCreatingNotification>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly NotificationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionCreatingNotificationConsumer(IHubContext<NotificationHub> hubContext,
    NotificationDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _hubContext = hubContext;
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<AuctionCreatingNotification> context)
    {
        Console.WriteLine("--> Получено сообщение - создан новый аукцион");
        //подписываем на получение сообщений автора аукциона
        var notifyItem = new NotifyUser { AuctionId = context.Message.Id, UserLogin = context.Message.AuctionAuthor };
        _dbContext.NotifyUser.Add(notifyItem);
        await _dbContext.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("AuctionCreated", context.Message);
        await _publishEndpoint.Publish(new AuctionCreatedNotification(context.Message.CorrelationId));
    }
}
