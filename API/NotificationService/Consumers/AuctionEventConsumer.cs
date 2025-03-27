using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Data;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class AuctionEventConsumer : IConsumer<DataForProcessingServicesList<NotifyItem>>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly NotificationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public AuctionEventConsumer(IHubContext<NotificationHub> hubContext,
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

        if (context.Message.DataObjects.Any())
        {
            foreach (var item in context.Message.DataObjects)
            {
                var auctionItem = JsonSerializer.Deserialize<AuctionItem>(item.Data);
                var notify = new AuctionNotification
                {
                    Title = auctionItem.Title,
                    CorrelationId = correlationId,
                    AuctionId = auctionItem.AuctionId,
                    UserLogin = auctionItem.Seller,
                    Show = Boolean.Parse(context.Message.Props)
                };

                switch (item.CRUD)
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
            }
        }
        else
        {
            await _hubContext.Clients.All.SendAsync("AuctionCreated", new AuctionNotification { Show = false });
        }
    }
}
