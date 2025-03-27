using System.Reflection;
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
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;
    private readonly NotificationDbContext _dbContext;

    public AuctionFinishedConsumer(IHubContext<NotificationHub> hubContext, NotificationDbContext dbContext,
        IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _hubContext = hubContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
        _dbContext = dbContext;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<AuctionItem>> context)
    {
        var correlationId = context.Message.CorrelationId;
        var title = !string.IsNullOrEmpty(context.Message.Props) ? context.Message.Props : "";
        foreach (var item in context.Message.DataObjects)
        {
            //уведомление о завершении аукциона
            var typedItem = JsonSerializer.Deserialize<AuctionItem>(item.Data);
            var auctionNotifyList = await _dbContext.NotifyItems.Where(p => p.AuctionId == typedItem.AuctionId).ToListAsync();
            await _hubContext.Clients.Groups(auctionNotifyList.Select(p => p.UserLogin)).SendAsync("AuctionFinished",
            new
            {
                auctionId = typedItem.AuctionId,
                winner = typedItem.Winner,
                amount = typedItem.SoldAmount,
                title = typedItem.Title
            });
        }

        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
        await _publishEndpoint.Publish(sendObject);
    }
}
