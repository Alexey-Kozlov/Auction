using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class BidPlacedErrorConsumer : IConsumer<Fault<RequestBidPlace>>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly NotificationDbContext _dbContext;

    public BidPlacedErrorConsumer(IHubContext<NotificationHub> hubContext, NotificationDbContext dbContext)
    {
        _hubContext = hubContext;
        _dbContext = dbContext;
    }
    public async Task Consume(ConsumeContext<Fault<RequestBidPlace>> context)
    {
        var auctionNotifyList = await _dbContext.NotifyUser.Where(p => p.AuctionId == context.Message.Message.AuctionId).ToListAsync();
        Console.WriteLine("--> Получено сообщение - ошибка создания ставки");
        await _hubContext.Clients.Groups(auctionNotifyList.Select(p => p.UserLogin)).SendAsync("FaultRequestBidPlace", context.Message.Message);
    }
}
