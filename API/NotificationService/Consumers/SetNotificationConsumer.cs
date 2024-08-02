using Common.Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Entities;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class SetNotificationConsumer : IConsumer<UserNotificationSet>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly NotificationDbContext _dbContext;

    public SetNotificationConsumer(IHubContext<NotificationHub> hubContext, NotificationDbContext dbContext)
    {
        _hubContext = hubContext;
        _dbContext = dbContext;
    }
    public async Task Consume(ConsumeContext<UserNotificationSet> context)
    {
        var userNotify = await _dbContext.NotifyUser.Where(p => p.AuctionId == context.Message.AuctionId &&
            p.UserLogin == context.Message.UserLogin).FirstOrDefaultAsync();
        if (userNotify == null)
        {
            _dbContext.NotifyUser.Add(new NotifyUser
            {
                AuctionId = context.Message.AuctionId,
                UserLogin = context.Message.UserLogin
            });
            Console.WriteLine("--> Добавлена рассылка уведомлений для " + context.Message.UserLogin);
        }
        else
        {
            Console.WriteLine("--> Рассылка для " + context.Message.UserLogin + " уже добавлена");
        }
        await context.Publish(new UserNotificationAdded(context.Message.CorrelationId));
        await _dbContext.SaveChangesAsync();
    }
}
