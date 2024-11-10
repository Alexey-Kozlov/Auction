using System.Reflection;
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
    private readonly IConfiguration _configuration;

    public AuctionNotificationConsumer(IHubContext<NotificationHub> hubContext,
    NotificationDbContext dbContext, IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _hubContext = hubContext;
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<ActionMessageList<NotifyItem>> context)
    {
        var correlationId = context.Message.ActionItemsList[0].CorrelationId;
        var auctionTitle = context.Message.Properties?[0] ?? "";
        var auctionCreatingNotification = new AuctionCreatingNotification(
            context.Message.ActionItemsList[0].ActionItem.AuctionId,
            context.Message.ActionItemsList[0].ActionItem.UserLogin,
            auctionTitle,
            Guid.NewGuid()
        );
        foreach (var actionItem in context.Message.ActionItemsList)
        {
            switch (actionItem.OperationType)
            {
                case OperationType.Delete:
                    var delItem = await _dbContext.NotifyItems.FirstOrDefaultAsync(p => p.AuctionId == actionItem.ActionItem.AuctionId &&
                        p.UserLogin == actionItem.ActionItem.UserLogin);
                    _dbContext.NotifyItems.Remove(delItem);
                    await _hubContext.Clients.Group(actionItem.ActionItem.UserLogin).SendAsync("AuctionDeleted", auctionCreatingNotification);
                    break;
                case OperationType.Insert:
                    _dbContext.NotifyItems.Add(actionItem.ActionItem);
                    await _hubContext.Clients.All.SendAsync("AuctionCreated", auctionCreatingNotification);
                    break;
                case OperationType.Bid:
                    var auctionNotifyList = await _dbContext.NotifyItems.Where(p => p.AuctionId == actionItem.ActionItem.AuctionId).ToListAsync();
                    //проверяем параметр Properties, если "true" - добавляем запись о рассылке уведомлений в БД
                    if (context.Message.Properties != null && context.Message.Properties[0] == "true")
                    {
                        _dbContext.NotifyItems.Add(actionItem.ActionItem);
                    }
                    await _hubContext.Clients.Groups(auctionNotifyList.Select(p => p.UserLogin)).SendAsync("BidPlaced", auctionCreatingNotification);
                    break;
                case OperationType.Update:
                    await _hubContext.Clients.Group(actionItem.ActionItem.UserLogin).SendAsync("AuctionUpdated", auctionCreatingNotification);
                    break;
            }
        }

        await _dbContext.SaveChangesAsync();
        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
        await _publishEndpoint.Publish(sendObject);
    }
}
