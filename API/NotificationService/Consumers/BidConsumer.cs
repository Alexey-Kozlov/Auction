using System.Reflection;
using System.Text.Json;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class BidConsumer : IConsumer<DataForProcessingServicesList<NotifyItem>>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly NotificationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public BidConsumer(IHubContext<NotificationHub> hubContext,
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
        foreach (var item in context.Message.DataObjects)
        {
            var typedItem = JsonSerializer.Deserialize<NotifyItem>(item.Data);
            if (item.CRUD == CRUD.Create)
            {
                await _dbContext.NotifyItems.AddAsync(new NotifyItem
                {
                    AuctionId = typedItem.AuctionId,
                    UserLogin = typedItem.UserLogin
                });
                await _dbContext.SaveChangesAsync();
            }

            var auctionNotifyList = await _dbContext.NotifyItems.Where(p => p.AuctionId == typedItem.AuctionId).ToListAsync();
            await _hubContext.Clients.Groups(auctionNotifyList.Select(p => p.UserLogin)).SendAsync("BidPlaced",
                new { auctionId = typedItem.AuctionId });

        }

        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
        await _publishEndpoint.Publish(sendObject);
    }

}
