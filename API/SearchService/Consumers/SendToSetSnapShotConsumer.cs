using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using Common.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;

namespace SearchService.Consumers;

public class SendToSetSnapShotConsumer : IConsumer<SendAllItems<SendToSetSnapShot>>
{
    private readonly IMapper _mapper;
    private readonly SearchDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public SendToSetSnapShotConsumer(IMapper mapper, SearchDbContext context, IPublishEndpoint publishEndpoint)
    {
        _mapper = mapper;
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
        var sendObject = new SendToSetSnapShot();
        var items = new List<AuctionItem>();
        _mapper.Map(await _context.Items.ToListAsync(), items);
        foreach (var item in items)
        {
            sendObject.SnapShotItems.Add(JsonSerializer.Serialize(item, item.GetType(), options));
        }
        sendObject.CorrelationId = consumeContext.Message.CorrelationId;
        sendObject.SessionId = consumeContext.Message.SessionId;
        sendObject.UserLogin = consumeContext.Message.UserLogin;
        sendObject.ItemsType = nameof(AuctionItem);
        sendObject.ProjectName = Assembly.GetExecutingAssembly().GetName().Name;
        sendObject.CreateAt = consumeContext.Message.CreateAt;
        await _publishEndpoint.Publish(sendObject);

        Console.WriteLine("--> Получение сообщения выполнить снапшот текущй БД в EventSourcing");
    }
}
