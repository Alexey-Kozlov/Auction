using Common.Contracts.Auction;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.UpdateAuctionStateMachine;

namespace ProcessingService.Activities.AuctionUpdate;

public class CommitActivity : IStateMachineActivity<UpdateAuctionState, AuctionUpdatedElk>
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


    public async Task Execute(BehaviorContext<UpdateAuctionState, AuctionUpdatedElk> context, IBehavior<UpdateAuctionState, AuctionUpdatedElk> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new RequestCommitESOperation(context.Saga.CorrelationId),
            nameof(CommitESOperation),
            "Common.Contracts.AuctionUpdateESCommit",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            Command.AuctionUpdate,
            context.Saga.AuctionId);
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