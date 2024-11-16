using System.Reflection;
using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using MassTransit;
using MassTransit.NewIdProviders;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class NotificationConsumer : IConsumer<DataForProcessingServicesList<NotifyItem>>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly NotificationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public NotificationConsumer(IHubContext<NotificationHub> hubContext,
    NotificationDbContext dbContext, IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _hubContext = hubContext;
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<NotifyItem>> context)
    {
        using var transaction = _dbContext.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
        var correlationId = context.Message.CorrelationId;
        foreach (var item in context.Message.DataObjects)
        {
            var typedItem = JsonSerializer.Deserialize<NotifyItem>(item.Data);
            var notify = new AuctionCreatingNotification
            (typedItem.AuctionId, typedItem.UserLogin, "", context.Message.CorrelationId);
            switch (item.CRUD)
            {
                case CRUD.Create:
                    await _dbContext.NotifyItems.AddAsync(new NotifyItem
                    {
                        AuctionId = typedItem.AuctionId,
                        UserLogin = typedItem.UserLogin
                    });
                    await _hubContext.Clients.All.SendAsync("AuctionCreated", notify);
                    break;
                case CRUD.Update:

                    break;
                case CRUD.Delete:
                    //удаляем запись
                    var delItem = await _dbContext.NotifyItems.Where(p =>
                        p.AuctionId == typedItem.AuctionId &&
                        p.UserLogin == typedItem.UserLogin).FirstOrDefaultAsync();
                    if (delItem == null)
                    {
                        throw new Exception($"Запись для удаления не найдена");
                    }
                    _dbContext.NotifyItems.Remove(delItem);
                    await _hubContext.Clients.Group(typedItem.UserLogin).SendAsync("AuctionDeleted", notify);
                    break;
            }
        }

        // foreach (var actionItem in context.Message.ActionItemsList)
        // {
        //     switch (actionItem.OperationType)
        //     {
        //         case OperationType.Delete:
        //             var delItem = await _dbContext.NotifyItems.FirstOrDefaultAsync(p => p.AuctionId == actionItem.ActionItem.AuctionId &&
        //                 p.UserLogin == actionItem.ActionItem.UserLogin);
        //             _dbContext.NotifyItems.Remove(delItem);
        //             await _hubContext.Clients.Group(actionItem.ActionItem.UserLogin).SendAsync("AuctionDeleted", auctionCreatingNotification);
        //             break;
        //         case OperationType.Insert:
        //             _dbContext.NotifyItems.Add(actionItem.ActionItem);
        //             await _hubContext.Clients.All.SendAsync("AuctionCreated", auctionCreatingNotification);
        //             break;
        //         case OperationType.Bid:
        //             var auctionNotifyList = await _dbContext.NotifyItems.Where(p => p.AuctionId == actionItem.ActionItem.AuctionId).ToListAsync();
        //             //проверяем параметр Properties, если "true" - добавляем запись о рассылке уведомлений в БД
        //             if (context.Message.Properties != null && context.Message.Properties[0] == "true")
        //             {
        //                 _dbContext.NotifyItems.Add(actionItem.ActionItem);
        //                 auctionNotifyList.Add(new NotifyItem
        //                 {
        //                     AuctionId = actionItem.ActionItem.AuctionId,
        //                     UserLogin = context.Message.ActionItemsList[0].ActionItem.UserLogin
        //                 });
        //             }
        //             await _hubContext.Clients.Groups(auctionNotifyList.Select(p => p.UserLogin)).SendAsync("BidPlaced", auctionCreatingNotification);
        //             break;
        //         case OperationType.Update:
        //             await _hubContext.Clients.Group(actionItem.ActionItem.UserLogin).SendAsync("AuctionUpdated", auctionCreatingNotification);
        //             break;
        //     }
        // }

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
        await _publishEndpoint.Publish(sendObject);
    }
}
