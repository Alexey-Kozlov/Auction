using Common.Contracts.Bid;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.BidPlacedStateMachine;

namespace ProcessingService.Activities.Bid;

public class CommitActivity : IStateMachineActivity<BidPlacedState, BidNotificationProcessed>
{
    private readonly SendEventToES _sendEventToES;
    private readonly IConfiguration _config;
    public CommitActivity(SendEventToES sendEventToES, IConfiguration config)
    {
        _sendEventToES = sendEventToES;
        _config = config;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }


    public async Task Execute(BehaviorContext<BidPlacedState, BidNotificationProcessed> context, IBehavior<BidPlacedState, BidNotificationProcessed> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new RequestCommitESOperation(context.Saga.CorrelationId),
            nameof(CommitESOperation),
            "Common.Contracts.BidCreateESCommit",
            context.Message.CorrelationId,
            context.Saga.Bidder,
            Command.PlaceBid,
            context.Saga.AuctionId);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<BidPlacedState, BidNotificationProcessed, TException> context, IBehavior<BidPlacedState, BidNotificationProcessed> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}