using Common.Contracts.Processing;
using MassTransit;
using MassTransit.Initializers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Hubs;
using ReportService.Services;

namespace NotificationService.Consumers;

public class EventConsumer : IConsumer<EventNotificationItem>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly NotificationDbContext _dbContext;
    private readonly GrpcUsersNotifyClient _grpcUsersNotifyClient;

    public EventConsumer(IHubContext<NotificationHub> hubContext, NotificationDbContext dbContext,
        GrpcUsersNotifyClient grpcUsersNotifyClient)
    {
        _hubContext = hubContext;
        _dbContext = dbContext;
        _grpcUsersNotifyClient = grpcUsersNotifyClient;
    }
    public async Task Consume(ConsumeContext<EventNotificationItem> context)
    {
        var notifyList = new List<string>();
        //await Task.Delay(3000);
        switch (context.Message.EventType)
        {
            //рассылка по всем пользователям
            case EventType.All:
                await _hubContext.Clients.All
                    .SendAsync(Enum.GetName(typeof(SignalRMethod), context.Message.SignalRMethod),
                    new
                    {
                        show = context.Message.Show,
                        data = context.Message.Data
                    });
                break;
            //рассылка по подписчикам на данный аукцион
            case EventType.AuctionGroup:
                var _users = await _dbContext.NotifyItems.Where(p =>
                        p.ItemId == context.Message.AuctionId && p.Commited)
                        .Select(p => p.UserLogin).ToListAsync();
                await _hubContext.Clients.Groups(_users)
                    .SendAsync(Enum.GetName(typeof(SignalRMethod), context.Message.SignalRMethod),
                        new
                        {
                            show = context.Message.Show,
                            data = context.Message.Data
                        });
                break;
            //рассылка по подписчикам на данный аукцион + текущий пользователь
            case EventType.AuctionGroup_UserLogin:
                var _users2 = await _dbContext.NotifyItems.Where(p =>
                        p.ItemId == context.Message.AuctionId && p.Commited)
                        .Select(p => p.UserLogin).ToListAsync();
                //если был указан UserLogin - добавить в рассылку (если нет)
                if (!string.IsNullOrEmpty(context.Message.UserLogin) &&
                    _users2.FirstOrDefault(p => p.ToLower() == context.Message.UserLogin.ToLower()) == null)
                {
                    _users2.Add(context.Message.UserLogin);
                }
                await _hubContext.Clients.Groups(_users2)
                    .SendAsync(Enum.GetName(typeof(SignalRMethod), context.Message.SignalRMethod),
                        new
                        {
                            show = context.Message.Show,
                            data = context.Message.Data
                        });
                break;
            //адресная рассылка для пользователей, у которых открыта указанная страница
            case EventType.Page:
                //получаем список пользователей, у которых открыта указанная страница, инфа из Редиса
                //Page - в переменной URL страницы, пользователям которой нужно отсылать уведомления
                var onLineUsers = await _grpcUsersNotifyClient.GetUserNotify(context.Message.Page);
                await _hubContext.Clients.Groups(onLineUsers)
                    .SendAsync(Enum.GetName(typeof(SignalRMethod), context.Message.SignalRMethod),
                    new
                    {
                        show = context.Message.Show,
                        data = context.Message.Data
                    });
                break;
            //адресная рассылка для пользователей, у которых открыта указанная страница +
            //рассылка по подписчикам на данный аукцион
            case EventType.AuctionGroup_Page:
                var pageUsers = await _grpcUsersNotifyClient.GetUserNotify(context.Message.Page);
                var groupUsers = await _dbContext.NotifyItems.Where(p =>
                        p.ItemId == context.Message.AuctionId && p.Commited)
                        .Select(p => p.UserLogin).ToListAsync();
                await _hubContext.Clients.Groups(pageUsers.Union(groupUsers))
                    .SendAsync(Enum.GetName(typeof(SignalRMethod), context.Message.SignalRMethod),
                    new
                    {
                        show = context.Message.Show,
                        data = context.Message.Data
                    });
                break;
            //здесь указано значение UserLogin пользователя, рассылка только этому пользователю     
            case EventType.UserLogin:
                await _hubContext.Clients.Groups(context.Message.UserLogin)
                    .SendAsync(Enum.GetName(typeof(SignalRMethod), context.Message.SignalRMethod),
                    new
                    {
                        show = context.Message.Show,
                        data = context.Message.Data
                    });
                break;
            default:
                break;
        }
    }
}
