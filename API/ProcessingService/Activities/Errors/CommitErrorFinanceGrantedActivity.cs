using Common.Contracts;
using MassTransit;
using ProcessingService.StateMachines.BidPlacedStateMachine;

namespace ProcessingService.Activities.Errors;

public class CommitErrorFinanceGrantedActivity : IStateMachineActivity<BidPlacedState, Fault<BidFinanceGranting>>
{
    private readonly ITopicProducer<BaseStateContract> _topicProducer;
    private readonly ILogger<CommitErrorFinanceGrantedActivity> _logger;
    public CommitErrorFinanceGrantedActivity(ITopicProducer<BaseStateContract> topicProducer,
        ILogger<CommitErrorFinanceGrantedActivity> logger)
    {
        _topicProducer = topicProducer;
        _logger = logger;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }


    public async Task Execute(BehaviorContext<BidPlacedState, Fault<BidFinanceGranting>> context, IBehavior<BidPlacedState, Fault<BidFinanceGranting>> next)
    {
        var message = new CommitBidErrorContract();
        message.CorrelationId = context.Saga.CorrelationId;
        message.Type = nameof(CommitBidErrorContract);
        message.Data = $"{context.Message.Exceptions[0].Message}";
        await _topicProducer.Produce(message);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<BidPlacedState, Fault<BidFinanceGranting>, TException> context, IBehavior<BidPlacedState, Fault<BidFinanceGranting>> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-create2");
    }
}