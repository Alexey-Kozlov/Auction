using Common.Contracts.Auction;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.UpdateAuctionStateMachine;


namespace ProcessingService.Activities.AuctionUpdate;

public class CommitActivity : IStateMachineActivity<UpdateAuctionState, AuctionUpdateESCommit>
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


    public async Task Execute(BehaviorContext<UpdateAuctionState, AuctionUpdateESCommit> context, IBehavior<UpdateAuctionState, AuctionUpdateESCommit> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new RequestCommitESOperation(context.Saga.CorrelationId),
            nameof(CommitESOperation),
            "Common.Contracts.Auction.AuctionUpdateComplete",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            Command.AuctionUpdate,
            "",
            context.Saga.AuctionId,
            !context.Message.IsError);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<UpdateAuctionState, AuctionUpdateESCommit, TException> context, IBehavior<UpdateAuctionState, AuctionUpdateESCommit> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}