using AuctionService.Bus;
using Common.Contracts;
using Confluent.Kafka;
using MassTransit;
using MassTransit.KafkaIntegration;

namespace AuctionService.Consumers;

public class TestConsumer : IConsumer<IMessage>
{
    private readonly ILogger<TestConsumer> _logger;

    public TestConsumer(ILogger<TestConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<IMessage> context)
    {
        var ctx = (context.ReceiveContext as KafkaReceiveContext<Ignore, IMessage>);
        _logger.LogInformation($"Message: {context.Message.Data}, Offset: {ctx?.Offset}");

        return Task.CompletedTask;
    }
}