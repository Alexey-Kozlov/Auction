using Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Data;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class BidFinanceGrantedFaultedConsumer : IConsumer<Fault<BidFinanceGranting>>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public BidFinanceGrantedFaultedConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task Consume(ConsumeContext<Fault<BidFinanceGranting>> context)
    {
        Console.WriteLine($"{DateTime.Now} --> Получено сообщение - ошибка создания ставки пользователя '{context.Message.Message.Bidder}'" +
        $" - не хватает денег, ставка - {context.Message.Message.Amount}, подробности в логе FinanceService");
        var message = new MessageContract
        (
            context.Message.Message.Id,
            $"Ошибка сервиса FinanceService, подробности в логе",
            MessageType.НедостаточноДенег,
            context.Message.Message.CorrelationId
        );
        await _hubContext.Clients.Group(context.Message.Message.Bidder).SendAsync("ErrorMessage", message);
    }
}
