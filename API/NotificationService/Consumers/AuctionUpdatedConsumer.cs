using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public AuctionUpdatedConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        Console.WriteLine("--> Получено сообщение - обновлен аукцион, послано сообщение " + context.Message.AuctionAuthor);
        await _hubContext.Clients.Group(context.Message.AuctionAuthor).SendAsync("AuctionUpdated", context.Message);
    }
}
