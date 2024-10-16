using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Contracts;
using MassTransit;
using ProcessingService.StateMachines.BidPlacedStateMachine;

namespace ProcessingService.Activities.BidPlaced;

public class BidPlacedActivity : IStateMachineActivity<BidPlacedState, RequestBidPlace>
{
    private readonly ITopicProducer<BaseStateContract> _topicProducer;
    private readonly ILogger<BidPlacedActivity> _logger;
    public BidPlacedActivity(ITopicProducer<BaseStateContract> topicProducer,
        ILogger<BidPlacedActivity> logger)
    {
        _topicProducer = topicProducer;
        _logger = logger;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<BidPlacedState, RequestBidPlace> context, IBehavior<BidPlacedState, RequestBidPlace> next)
    {
        _logger.LogInformation($"{DateTime.Now} Команда на создание записи в EventSourcing - команда BidPlaced");
        JsonSerializerOptions options = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        var message = new BidPlacedStateContract();
        message.CorrelationId = context.Saga.CorrelationId;
        message.Type = nameof(BidPlacedStateContract);
        message.Data = JsonSerializer.Serialize(context.Saga, context.Saga.GetType(), options);
        await _topicProducer.Produce(message);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<BidPlacedState, RequestBidPlace, TException> context, IBehavior<BidPlacedState, RequestBidPlace> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-bidplace");
    }
}