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
        Console.WriteLine($"{DateTime.Now} --> Получено сообщение - ошибка создания ставки пользователя '{context.Message.Message.Bidder}'" +
        $" , ошибка обновления записи новой ставки '{context.Message.Message.Amount}' в аукционе");
        var error = new ErrorContract
        (
            context.Message.Message.Id,
            $"Ошибка создания ставки пользователя {context.Message.Message.Bidder}" +
            $" , ошибка обновления записи новой ставки '{context.Message.Message.Amount}'",
            context.Message.Message.CorrelationId
        );
        await _hubContext.Clients.Group(context.Message.Message.Bidder).SendAsync("ErrorMessage", error);
    }
}
