using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;

namespace NotificationService.Consumers;

public class SendToSetSnapShotConsumer : IConsumer<SendAllItems<SendToSetSnapShot>>
{
    private readonly NotificationDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public SendToSetSnapShotConsumer(NotificationDbContext context, IPublishEndpoint publishEndpoint)
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
        var notifObject = new SendToSetSnapShot();
        var items = new List<AuctionItem>();
        foreach (var item in await _context.NotifyItems.ToArrayAsync())
        {
            notifObject.SnapShotItems.Add(JsonSerializer.Serialize(item, item.GetType(), options));
        }
        notifObject.CorrelationId = consumeContext.Message.CorrelationId;
        notifObject.SessionId = consumeContext.Message.SessionId;
        notifObject.UserLogin = consumeContext.Message.UserLogin;
        notifObject.ItemsType = nameof(NotifyItem);
        notifObject.CreateAt = consumeContext.Message.CreateAt;
        await _publishEndpoint.Publish(notifObject);

        Console.WriteLine("--> Получение сообщения выполнить снапшот текущй БД в EventSourcing");
    }
}
