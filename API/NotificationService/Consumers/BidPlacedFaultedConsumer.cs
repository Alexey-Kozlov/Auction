using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class BidPlacedFaultedConsumer : IConsumer<Fault<BidPlacing>>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public BidPlacedFaultedConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task Consume(ConsumeContext<Fault<BidPlacing>> context)
    {
        Console.WriteLine("--> Получено сообщение - ошибка создания ставки");
        await _hubContext.Clients.Groups(new List<string> { context.Message.Message.Bidder }).SendAsync("FaultRequestBid", context.Message.Message);
    }
}
