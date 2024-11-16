using Common.Contracts.Auction;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.CreateAuctionStateMachine;

namespace ProcessingService.Activities.AuctionCreate;

public class CommitActivity : IStateMachineActivity<CreateAuctionState, AuctionCreateESCommit>
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


    public async Task Execute(BehaviorContext<CreateAuctionState, AuctionCreateESCommit> context, IBehavior<CreateAuctionState, AuctionCreateESCommit> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new RequestCommitESOperation(context.Saga.CorrelationId),
            nameof(CommitESOperation),
            "Common.Contracts.Auction.AuctionCreateComplete",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            Command.AuctionCreate,
            context.Saga.AuctionId);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<CreateAuctionState, AuctionCreateESCommit, TException> context, IBehavior<CreateAuctionState, AuctionCreateESCommit> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}