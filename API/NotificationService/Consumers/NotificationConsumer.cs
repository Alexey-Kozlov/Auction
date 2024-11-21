using System.Reflection;
using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Bid;
using Common.Contracts.ELKSearch;
using Common.Contracts.Finance;
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
        using var transaction = _dbContext.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
        var correlationId = context.Message.CorrelationId;
        var title = !string.IsNullOrEmpty(context.Message.Props) ? context.Message.Props : "";
        foreach (var item in context.Message.DataObjects)
        {

            //уведомления при операциях с аукционом - создание, обновление, удаление
            await ProcessNotifyItem(item, title, correlationId);


            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
            await _publishEndpoint.Publish(sendObject);
        }
    }

    private async Task ProcessNotifyItem(DataForProcessingService item, string title, Guid correlationId)
    {
        var typedItem = JsonSerializer.Deserialize<NotifyItem>(item.Data);
        var notify = new AuctionNotification
        {
            Title = title,
            CorrelationId = correlationId,
            AuctionId = typedItem.AuctionId,
            UserLogin = typedItem.UserLogin
        };

        switch (item.CRUD)
        {
            case CRUD.Create:
                await _dbContext.NotifyItems.AddAsync(typedItem);
                await _hubContext.Clients.All.SendAsync("AuctionCreated", notify);
                break;
            case CRUD.Update:
                await _hubContext.Clients.Group(typedItem.UserLogin).SendAsync("AuctionUpdated", notify);
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

}
