using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Contracts;
using FinanceService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Consumers;

public class SendToSetSnapShotConsumer : IConsumer<SendAllItems<SendToSetSnapShot>>
{
    private readonly FinanceDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public SendToSetSnapShotConsumer(FinanceDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<SendAllItems<SendToSetSnapShot>> consumeContext)
    {
        JsonSerializerOptions options = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        var sendBalanse = new SendToSetSnapShot();
        //получаем записи по таблице FinanceItem
        foreach (var item in await _context.FinanceItems.ToListAsync())
        {
            sendBalanse.SnapShotItems.Add(JsonSerializer.Serialize(item, item.GetType(), options));
        }
        sendBalanse.CorrelationId = consumeContext.Message.CorrelationId;
        sendBalanse.SessionId = consumeContext.Message.SessionId;
        sendBalanse.UserLogin = consumeContext.Message.UserLogin;
        sendBalanse.ItemsType = nameof(FinanceItem);
        sendBalanse.ServiceName = Assembly.GetExecutingAssembly().GetName().Name;
        sendBalanse.CreateAt = consumeContext.Message.CreateAt;
        sendBalanse.RestoringOrder = 1;
        await _publishEndpoint.Publish(sendBalanse);

        Console.WriteLine("--> Получение сообщения выполнить снапшот текущй БД в EventSourcing");
    }
}

