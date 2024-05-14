using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class BidSearchPlacedFaultedConsumer : IConsumer<Fault<BidSearchPlacing>>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public BidSearchPlacedFaultedConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task Consume(ConsumeContext<Fault<BidSearchPlacing>> context)
    {
        Console.WriteLine($"{DateTime.Now} --> Получено сообщение - ошибка обновления поиска аукциона для новой ставки пользователя '{context.Message.Message.Bidder}'" +
            $" , ошибка размещения ставки пользователя, ставка - {context.Message.Message.Amount}");
        var error = new ErrorContract
        (
            context.Message.Message.Id,
            $"Ошибка обновления поиска аукциона для новой ставки пользователя {context.Message.Message.Bidder}" +
            $" , ошибка размещения ставки пользователя, ставка - {context.Message.Message.Amount}",
            context.Message.Message.CorrelationId
        );
        await _hubContext.Clients.Group(context.Message.Message.Bidder).SendAsync("ErrorMessage", error);
    }
}
