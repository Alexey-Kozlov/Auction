using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using MassTransit;

namespace Common.Utils;

public class SendEventToES
{
    private readonly ITopicProducer<ESContract> _topicProducer;
    public SendEventToES(ITopicProducer<ESContract> topicProducer)
    {
        _topicProducer = topicProducer;
    }

    public async Task SendItemToEventSourcing<T>(T context, string typeName,
        string callBackType, Guid correlationId, string userLogin, Command command, string image,
        Guid? auctionId, Guid? ItemId, bool isError, string errorMessage, string errorExceptionMessage, string errorServiceName)
    {
        JsonSerializerOptions options = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        var message = new ESContract();
        message.EventData = JsonSerializer.Serialize(context, context.GetType(), options);
        message.EntityType = typeName;
        message.CallBackType = callBackType;
        message.CorrelationId = correlationId;
        message.AuctionId = auctionId;
        message.ItemId = ItemId;
        message.UserLogin = userLogin;
        message.Command = command;
        message.Image = image;
        message.IsError = isError;
        message.ErrorMessage = errorMessage;
        message.ErrorExceptionMessage = errorExceptionMessage;
        message.ErrorServiceName = errorServiceName;
        await _topicProducer.Produce(message);
    }
}