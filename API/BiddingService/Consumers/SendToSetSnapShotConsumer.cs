using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using BiddingService.Data;
using Common.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BiddingService.Consumers;

public class SendToSetSnapShotConsumer : IConsumer<SendAllItems<SendToSetSnapShot>>
{
    private readonly IMapper _mapper;
    private readonly BidDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public SendToSetSnapShotConsumer(IMapper mapper, BidDbContext context, IPublishEndpoint publishEndpoint)
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
        var sendAuctions = new SendToSetSnapShot();
        //получаем записи по таблице Auctions
        foreach (var item in await _context.Auctions.ToListAsync())
        {
            sendAuctions.SnapShotItems.Add(JsonSerializer.Serialize(item, item.GetType(), options));
        }
        sendAuctions.CorrelationId = consumeContext.Message.CorrelationId;
        sendAuctions.SessionId = consumeContext.Message.SessionId;
        sendAuctions.UserLogin = consumeContext.Message.UserLogin;
        sendAuctions.ItemsType = nameof(AuctionBidItem);
        sendAuctions.CreateAt = consumeContext.Message.CreateAt;
        sendAuctions.RestoringOrder = 1;

        await _publishEndpoint.Publish(sendAuctions);

        var sendBids = new SendToSetSnapShot();
        //получаем записи по таблице Bids
        foreach (var item in await _context.Bids.ToListAsync())
        {
            var rez = JsonSerializer.Serialize(item, item.GetType(), options);
            sendBids.SnapShotItems.Add(JsonSerializer.Serialize(item, item.GetType(), options));
        }
        sendBids.CorrelationId = consumeContext.Message.CorrelationId;
        sendBids.SessionId = consumeContext.Message.SessionId;
        sendBids.UserLogin = consumeContext.Message.UserLogin;
        sendBids.ItemsType = nameof(BidItem);
        sendBids.CreateAt = consumeContext.Message.CreateAt;
        sendBids.RestoringOrder = 2;
        await _publishEndpoint.Publish(sendBids);

        Console.WriteLine("--> Получение сообщения выполнить снапшот текущй БД в EventSourcing");
    }
}

