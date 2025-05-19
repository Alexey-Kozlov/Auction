using Common.Contracts.Logging;
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
        await _topicProducer.Produce(message);
    }
}