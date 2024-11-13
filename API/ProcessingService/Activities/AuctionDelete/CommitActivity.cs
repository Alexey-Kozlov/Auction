using Common.Contracts.Auction;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.DeleteAuctionStateMachine;

namespace ProcessingService.Activities.AuctionDelete;

public class CommitActivity : IStateMachineActivity<DeleteAuctionState, AuctionDeletedNotification>
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


    public async Task Execute(BehaviorContext<DeleteAuctionState, AuctionDeletedNotification> context, IBehavior<DeleteAuctionState, AuctionDeletedNotification> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new RequestCommitESOperation(context.Saga.CorrelationId),
            nameof(CommitESOperation),
            "Common.Contracts.AuctionDeleteESCommit",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            OperationType.AuctionDelete,
            context.Saga.AuctionId);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<DeleteAuctionState, AuctionDeletedNotification, TException> context, IBehavior<DeleteAuctionState, AuctionDeletedNotification> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}