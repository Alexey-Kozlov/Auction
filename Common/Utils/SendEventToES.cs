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
        Guid? auctionId, Guid? itemId, bool isError, string errorMessage, string errorExceptionStack,
        string errorExceptionInputData, string errorServiceName)
    {
        JsonSerializerOptions options = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        var message = new ESContract
        {
            EventData = JsonSerializer.Serialize(context, context.GetType(), options),
            EntityType = typeName,
            CallBackType = callBackType,
            CorrelationId = correlationId,
            AuctionId = auctionId,
            ItemId = itemId,
            UserLogin = userLogin,
            Command = command,
            Image = image,
            IsError = isError,
            ErrorMessage = errorMessage,
            ErrorExceptionStack = errorExceptionStack,
            ErrorExceptionInputData = errorExceptionInputData,
            ErrorServiceName = errorServiceName
        };
        await _topicProducer.Produce(message);
    }
}