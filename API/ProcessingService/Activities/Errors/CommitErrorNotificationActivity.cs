using Common.Contracts;
using MassTransit;
using ProcessingService.StateMachines.BidPlacedStateMachine;

namespace ProcessingService.Activities.Errors;

public class CommitErrorNotificationActivity : IStateMachineActivity<BidPlacedState, Fault<BidNotificationProcessing>>
{
    private readonly ITopicProducer<BaseStateContract> _topicProducer;
    private readonly ILogger<CommitErrorNotificationActivity> _logger;
    public CommitErrorNotificationActivity(ITopicProducer<BaseStateContract> topicProducer,
        ILogger<CommitErrorNotificationActivity> logger)
    {
        _topicProducer = topicProducer;
        _logger = logger;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }


    public async Task Execute(BehaviorContext<BidPlacedState, Fault<BidNotificationProcessing>> context, IBehavior<BidPlacedState, Fault<BidNotificationProcessing>> next)
    {
        var message = new CommitBidErrorContract();
        message.CorrelationId = context.Saga.CorrelationId;
        message.Type = nameof(CommitBidErrorContract);
        message.Data = context.Message.Exceptions[0].Message;
        await _topicProducer.Produce(message);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<BidPlacedState, Fault<BidNotificationProcessing>, TException> context, IBehavior<BidPlacedState, Fault<BidNotificationProcessing>> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-create2");
    }
}