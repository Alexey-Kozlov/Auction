using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Contracts;
using MassTransit;

namespace Common.Utils;

public class SendEventToES
{
    private readonly ITopicProducer<BaseStateContract> _topicProducer;
    public SendEventToES(ITopicProducer<BaseStateContract> topicProducer)
    {
        _topicProducer = topicProducer;
    }

    public async Task SendItemToEventSourcing<T>(T context, string typeName,
        string serviceName, Guid correlationId, string userLogin, Guid auctionId)
    {
        JsonSerializerOptions options = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        var message = new BaseStateContract();
        message.EventData = JsonSerializer.Serialize(context, context.GetType(), options);
        message.EntityType = typeName;
        message.ServiceName = serviceName;
        message.CorrelationId = correlationId;
        message.AuctionId = auctionId;
        message.UserLogin = userLogin;
        await _topicProducer.Produce(message);
    }
}