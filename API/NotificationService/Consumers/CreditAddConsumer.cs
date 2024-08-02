using Common.Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Data;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class CreditAddConsumer : IConsumer<FinanceCreditAdd>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly NotificationDbContext _dbContext;

    public CreditAddConsumer(IHubContext<NotificationHub> hubContext, NotificationDbContext dbContext)
    {
        _hubContext = hubContext;
        _dbContext = dbContext;
    }
    public async Task Consume(ConsumeContext<FinanceCreditAdd> context)
    {
        Console.WriteLine("--> Получено сообщение - пополнен кредит пользователя, пользователь " + context.Message.UserLogin +
        ", сумма - " + context.Message.Credit);
        await _hubContext.Clients.Group(context.Message.UserLogin).SendAsync("FinanceCreditAdd", context.Message);
    }
}
