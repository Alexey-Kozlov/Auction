using Common.Contracts.Auction;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.FinishAuctionStateMachine;



namespace ProcessingService.Activities.AuctionFinish;

public class CommitActivity : IStateMachineActivity<FinishAuctionState, AuctionFinishedESCommit>
{
    private readonly SendEventToES _sendEventToES;
    public CommitActivity(SendEventToES sendEventToES)
    {
        _sendEventToES = sendEventToES;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }


    public async Task Execute(BehaviorContext<FinishAuctionState, AuctionFinishedESCommit> context, IBehavior<FinishAuctionState, AuctionFinishedESCommit> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new RequestCommitESOperation(context.Saga.CorrelationId),
            nameof(CommitESOperation),
            "Common.Contracts.Auction.AuctionFinishedComplete",
            context.Message.CorrelationId,
            "",
            Command.AuctionUpdate,
            "",
            null);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<FinishAuctionState, AuctionFinishedESCommit, TException> context, IBehavior<FinishAuctionState, AuctionFinishedESCommit> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}