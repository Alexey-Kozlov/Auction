using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class BidAuctionPlacedFaultedConsumer : IConsumer<Fault<BidAuctionPlacing>>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public BidAuctionPlacedFaultedConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task Consume(ConsumeContext<Fault<BidAuctionPlacing>> context)
    {
        Console.WriteLine("--> Получено сообщение - ошибка создания ставки");
        await _hubContext.Clients.Groups(new List<string> { context.Message.Message.Bidder }).SendAsync("FaultRequestBid", context.Message.Message);
    }
}
