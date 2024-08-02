using Common.Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class AuctionFinishingNotificationConsumer : IConsumer<AuctionFinishingNotification>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly NotificationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionFinishingNotificationConsumer(IHubContext<NotificationHub> hubContext,
        NotificationDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _hubContext = hubContext;
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<AuctionFinishingNotification> context)
    {
        var auctionNotifyList = await _dbContext.NotifyUser.Where(p => p.AuctionId == context.Message.Id).ToListAsync();
        Console.WriteLine($"{DateTime.Now} --> Получено сообщение - аукцион '{context.Message.Id}' завершен, рассылка уведомлений для " +
            $"{String.Join(',', auctionNotifyList.Select(p => p.UserLogin))}");
        await _hubContext.Clients.Groups(auctionNotifyList.Select(p => p.UserLogin)).SendAsync("AuctionFinished", context.Message);
        await _publishEndpoint.Publish(new AuctionFinishedNotification(context.Message.CorrelationId));
    }
}
