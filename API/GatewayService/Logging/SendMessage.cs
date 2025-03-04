using Common.Contracts;
using MassTransit;

namespace GatewayService.Logging;

public class SendMessage
{
    private readonly ITopicProducer<ItemLoggingContract> _topicProducer;
    public SendMessage(ITopicProducer<ItemLoggingContract> topicProducer)
    {
        _topicProducer = topicProducer;
    }

    public async Task SendLogTopic(ItemLoggingContract message)
    {
        if (string.IsNullOrEmpty(message.RequestId)) return;
        await _topicProducer.Produce(message);
    }
}