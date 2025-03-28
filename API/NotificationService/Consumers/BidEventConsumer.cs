using System.Text.Json;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class BidEventConsumer : IConsumer<DataForProcessingServicesList<NotifyItem>>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly NotificationDbContext _dbContext;

    public BidEventConsumer(IHubContext<NotificationHub> hubContext, NotificationDbContext dbContext)
    {
        _hubContext = hubContext;
        _dbContext = dbContext;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<NotifyItem>> context)
    {
        var correlationId = context.Message.CorrelationId;

        if (context.Message.DataObjects.Any())
        {
            foreach (var item in context.Message.DataObjects)
            {
                var typedItem = JsonSerializer.Deserialize<NotifyItem>(item.Data);
                var auctionNotifyList = await _dbContext.NotifyItems.Where(p =>
                    p.AuctionId == typedItem.AuctionId && p.Commited).ToListAsync();
                await _hubContext.Clients.Groups(auctionNotifyList.Select(p => p.UserLogin)).SendAsync("BidPlaced",
                    new { auctionId = typedItem.AuctionId, show = Boolean.Parse(context.Message.Props) });
            }
        }
        else
        {
            await _hubContext.Clients.All.SendAsync("BidPlaced", new { show = false });
        }
    }

}
