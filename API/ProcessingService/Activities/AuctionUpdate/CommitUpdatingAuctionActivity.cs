using Common.Contracts;
using MassTransit;
using ProcessingService.StateMachines.UpdateAuctionStateMachine;

namespace ProcessingService.Activities.AuctionUpdate;

public class CommitUpdatingAuctionActivity : IStateMachineActivity<UpdateAuctionState, AuctionUpdatedElk>
{
    private readonly ITopicProducer<BaseStateContract> _topicProducer;
    private readonly ILogger<CommitUpdatingAuctionActivity> _logger;
    public CommitUpdatingAuctionActivity(ITopicProducer<BaseStateContract> topicProducer,
        ILogger<CommitUpdatingAuctionActivity> logger)
    {
        _topicProducer = topicProducer;
        _logger = logger;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }


    public async Task Execute(BehaviorContext<UpdateAuctionState, AuctionUpdatedElk> context, IBehavior<UpdateAuctionState, AuctionUpdatedElk> next)
    {
        var message = new CommitAuctionUpdatingContract();
        message.CorrelationId = context.Saga.CorrelationId;
        message.Type = nameof(CommitAuctionUpdatingContract);
        await _topicProducer.Produce(message);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<UpdateAuctionState, AuctionUpdatedElk, TException> context, IBehavior<UpdateAuctionState, AuctionUpdatedElk> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}