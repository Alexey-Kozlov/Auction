using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Data;
using NotificationService.Entities;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly NotificationDbContext _dbContext;

    public AuctionCreatedConsumer(IHubContext<NotificationHub> hubContext, NotificationDbContext dbContext)
    {
        _hubContext = hubContext;
        _dbContext = dbContext;
    }
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine("--> Получено сообщение - создан новый аукцион");
        //подписываем на получение сообщений автора аукциона
        var notifyItem = new NotifyUser { AuctionId = context.Message.Id, UserLogin = context.Message.Seller };
        _dbContext.NotifyUser.Add(notifyItem);
        await _dbContext.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("AuctionCreated", context.Message);
    }
}
