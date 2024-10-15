using Common.Contracts;
using MassTransit;
using ProcessingService.StateMachines.BidPlacedStateMachine;

namespace ProcessingService.Activities.BidPlaced;

public class CommitBidPlacedActivity : IStateMachineActivity<BidPlacedState, BidNotificationProcessed>
{
    private readonly ITopicProducer<BaseStateContract> _topicProducer;
    private readonly ILogger<CommitBidPlacedActivity> _logger;
    public CommitBidPlacedActivity(ITopicProducer<BaseStateContract> topicProducer,
        ILogger<CommitBidPlacedActivity> logger)
    {
        _topicProducer = topicProducer;
        _logger = logger;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }


    public async Task Execute(BehaviorContext<BidPlacedState, BidNotificationProcessed> context, IBehavior<BidPlacedState, BidNotificationProcessed> next)
    {
        var message = new CommitBidPlacingContract();
        message.CorrelationId = context.Saga.CorrelationId;
        message.Type = nameof(CommitBidPlacingContract);
        await _topicProducer.Produce(message);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<BidPlacedState, BidNotificationProcessed, TException> context, IBehavior<BidPlacedState, BidNotificationProcessed> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-create2");
    }
}