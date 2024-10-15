using Common.Contracts;
using MassTransit;
using ProcessingService.StateMachines.DeleteAuctionStateMachine;

namespace ProcessingService.Activities.AuctionDelete;

public class CommitDeletingAuctionActivity : IStateMachineActivity<DeleteAuctionState, AuctionDeletedElk>
{
    private readonly ITopicProducer<BaseStateContract> _topicProducer;
    private readonly ILogger<CommitDeletingAuctionActivity> _logger;
    public CommitDeletingAuctionActivity(ITopicProducer<BaseStateContract> topicProducer,
        ILogger<CommitDeletingAuctionActivity> logger)
    {
        _topicProducer = topicProducer;
        _logger = logger;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }


    public async Task Execute(BehaviorContext<DeleteAuctionState, AuctionDeletedElk> context, IBehavior<DeleteAuctionState, AuctionDeletedElk> next)
    {
        var message = new CommitAuctionDeletingContract();
        message.CorrelationId = context.Saga.CorrelationId;
        message.Type = nameof(CommitAuctionDeletingContract);
        await _topicProducer.Produce(message);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<DeleteAuctionState, AuctionDeletedElk, TException> context, IBehavior<DeleteAuctionState, AuctionDeletedElk> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}