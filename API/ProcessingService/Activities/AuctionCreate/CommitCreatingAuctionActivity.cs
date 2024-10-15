using Common.Contracts;
using MassTransit;
using ProcessingService.StateMachines.CreateAuctionStateMachine;

namespace ProcessingService.Activities.AuctionCreate;

public class CommitCreatingAuctionActivity : IStateMachineActivity<CreateAuctionState, AuctionCreatedElk>
{
    private readonly ITopicProducer<BaseStateContract> _topicProducer;
    private readonly ILogger<CommitCreatingAuctionActivity> _logger;
    public CommitCreatingAuctionActivity(ITopicProducer<BaseStateContract> topicProducer,
        ILogger<CommitCreatingAuctionActivity> logger)
    {
        _topicProducer = topicProducer;
        _logger = logger;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }


    public async Task Execute(BehaviorContext<CreateAuctionState, AuctionCreatedElk> context, IBehavior<CreateAuctionState, AuctionCreatedElk> next)
    {
        var message = new CommitAuctionCreatingContract();
        message.CorrelationId = context.Saga.CorrelationId;
        message.Type = nameof(CommitAuctionCreatingContract);
        await _topicProducer.Produce(message);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<CreateAuctionState, AuctionCreatedElk, TException> context, IBehavior<CreateAuctionState, AuctionCreatedElk> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}