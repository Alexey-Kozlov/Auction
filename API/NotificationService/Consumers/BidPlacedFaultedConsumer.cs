using Common.Contracts;
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
        Console.WriteLine($"{DateTime.Now} --> Получено сообщение - ошибка создания ставки пользователя '{context.Message.Message.Bidder}'" +
            $" , ошибка размещения ставки пользователя, подробности в логе BidService");

        var message = new MessageContract
        (
            context.Message.Message.Id,
            $"Ошибка сервиса BidService, подробности в логе",
            MessageType.Ошибка,
            context.Message.Message.CorrelationId
        );
        await _hubContext.Clients.Group(context.Message.Message.Bidder).SendAsync("ErrorMessage", message);
    }
}
