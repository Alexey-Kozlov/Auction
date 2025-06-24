using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Processing;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class AuctionFinishedConsumer : IConsumer<DataForProcessingServicesList<AuctionItem>>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly NotificationDbContext _dbContext;

    public AuctionFinishedConsumer(IHubContext<NotificationHub> hubContext, NotificationDbContext dbContext)
    {
        _hubContext = hubContext;
        _dbContext = dbContext;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<AuctionItem>> context)
    {
        foreach (var item in context.Message.DataObjects)
        {
            //уведомление о завершении аукциона
            var typedItem = JsonSerializer.Deserialize<AuctionItem>(item.Data);
            var auctionNotifyList = await _dbContext.NotifyItems.Where(p => p.ItemId == typedItem.AuctionId).ToListAsync();
            await _hubContext.Clients.Groups(auctionNotifyList.Select(p => p.UserLogin)).SendAsync("AuctionFinished",
            new
            {
                auctionId = typedItem.AuctionId,
                winner = typedItem.Winner,
                amount = typedItem.SoldAmount,
                title = typedItem.Title
            });
        }
    }
}
