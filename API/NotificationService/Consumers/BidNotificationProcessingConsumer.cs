using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class BidNotificationProcessingConsumer : IConsumer<BidNotificationProcessing>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly NotificationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public BidNotificationProcessingConsumer(IHubContext<NotificationHub> hubContext,
    NotificationDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _hubContext = hubContext;
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<BidNotificationProcessing> context)
    {
        var auctionNotifyList = await _dbContext.NotifyUser.Where(p => p.AuctionId == context.Message.AuctionId).ToListAsync();
        Console.WriteLine("--> Получено сообщение - заявка размещена, рассылка уведомлений для " + String.Join(',', auctionNotifyList.Select(p => p.UserLogin)));
        await _hubContext.Clients.Groups(auctionNotifyList.Select(p => p.UserLogin)).SendAsync("BidPlaced", context.Message);
        await _publishEndpoint.Publish(new BidNotificationProcessed(context.Message.CorrelationId));
    }
}
