using Common.Contracts;
using MassTransit;
using ProcessingService.StateMachines.FinishAuctionStateMachine;

namespace ProcessingService.Activities.AuctionDelete;

public class CommitFinishingAuctionActivity : IStateMachineActivity<FinishAuctionState, AuctionFinishedElk>
{
    private readonly ITopicProducer<BaseStateContract> _topicProducer;
    private readonly ILogger<CommitFinishingAuctionActivity> _logger;
    public CommitFinishingAuctionActivity(ITopicProducer<BaseStateContract> topicProducer,
        ILogger<CommitFinishingAuctionActivity> logger)
    {
        _topicProducer = topicProducer;
        _logger = logger;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }


    public async Task Execute(BehaviorContext<FinishAuctionState, AuctionFinishedElk> context, IBehavior<FinishAuctionState, AuctionFinishedElk> next)
    {
        var message = new CommitAuctionFinishingContract();
        message.CorrelationId = context.Saga.CorrelationId;
        message.Type = nameof(CommitAuctionFinishingContract);
        await _topicProducer.Produce(message);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<FinishAuctionState, AuctionFinishedElk, TException> context, IBehavior<FinishAuctionState, AuctionFinishedElk> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-finish");
    }
}