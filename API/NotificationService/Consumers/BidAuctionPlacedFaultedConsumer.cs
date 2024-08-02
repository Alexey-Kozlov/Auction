using Common.Contracts;
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
        Console.WriteLine($"{DateTime.Now} --> Получено сообщение - ошибка создания ставки пользователя '{context.Message.Message.Bidder}'" +
        $" , подробности в логе AuctionService");
        var message = new MessageContract
        (
            context.Message.Message.Id,
            $"Ошибка сервиса AuctionService, подробности в логе",
            MessageType.Ошибка,
            context.Message.Message.CorrelationId
        );
        await _hubContext.Clients.Group(context.Message.Message.Bidder).SendAsync("ErrorMessage", message);
    }
}
