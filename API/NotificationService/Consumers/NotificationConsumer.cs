using System.Reflection;
using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using MassTransit;
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
        var correlationId = context.Message.CorrelationId;
        try
        {
            foreach (var item in context.Message.DataObjects)
            {
                //уведомления при операциях с аукционом - создание, обновление, удаление
                await ProcessNotifyItem(item, correlationId);
                await _dbContext.SaveChangesAsync();
            }
            var auctionData = JsonSerializer.Deserialize<AuctionNotificationData>(context.Message.Props);
            var auctionItem = JsonSerializer.Deserialize<AuctionItem>(auctionData.AuctionData);
            var notify = new AuctionNotification
            {
                Title = auctionItem.Title,
                CorrelationId = correlationId,
                AuctionId = auctionItem.AuctionId,
                UserLogin = auctionItem.Seller
            };

            switch (auctionData.CRUD)
            {
                case CRUD.Create:
                    await _hubContext.Clients.All.SendAsync("AuctionCreated", notify);
                    break;
                case CRUD.Update:
                    await _hubContext.Clients.Group(auctionItem.Seller).SendAsync("AuctionUpdated", notify);
                    break;
                case CRUD.Delete:
                    await _hubContext.Clients.Group(auctionItem.Seller).SendAsync("AuctionDeleted", notify);
                    break;
            }
            var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                 _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
            await _publishEndpoint.Publish(sendObject);
        }
        catch (Exception e)
        {
            //ошибки, в т.ч. штатные
            var messageObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            messageObject.GetType().GetProperty("CorrelationId").SetValue(messageObject, context.Message.CorrelationId);
            messageObject.GetType().GetProperty("Message").SetValue(messageObject, e.Message);
            messageObject.GetType().GetProperty("ExceptionMessage").SetValue(messageObject, e.StackTrace);
            messageObject.GetType().GetProperty("ServiceName").SetValue(messageObject, "NotificationService");
            messageObject.GetType().GetProperty("UserLogin").SetValue(messageObject, "");
            messageObject.GetType().GetProperty("AuctionId").SetValue(messageObject, null);
            messageObject.GetType().GetProperty("IsError").SetValue(messageObject, true);
            var faultType = typeof(FaultMessage<>);
            var typeParams = new Type[] { messageObject.GetType() };
            var faultObjectType = faultType.MakeGenericType(typeParams);

            var faultObject = Activator.CreateInstance(faultObjectType,
                new object[] { "", messageObject });

            await _publishEndpoint.Publish(faultObject);
        }
    }

    private async Task ProcessNotifyItem(DataForProcessingService item, Guid correlationId)
    {
        var typedItem = JsonSerializer.Deserialize<NotifyItem>(item.Data);

        switch (item.CRUD)
        {
            case CRUD.Create:
                await _dbContext.NotifyItems.AddAsync(typedItem);
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
                break;
        }
    }

}
