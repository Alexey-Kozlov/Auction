using Common.Contracts.Auction;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.DeleteAuctionStateMachine;

namespace ProcessingService.Activities.AuctionDelete;

public class ESLogActivity : IStateMachineActivity<DeleteAuctionState, RequestAuctionDelete>
{
    private readonly SendEventToES _sendEventToES;
    public ESLogActivity(SendEventToES sendEventToES)
    {
        _sendEventToES = sendEventToES;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<DeleteAuctionState, RequestAuctionDelete> context, IBehavior<DeleteAuctionState, RequestAuctionDelete> next)
    {
        await _sendEventToES.SendItemToEventSourcing<object>(
            new object(),
            nameof(AuctionDelete),
            "Common.Contracts.Processing.ESLogAuctionDeleted",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            Command.AuctionDelete,
            "",
            context.Saga.ItemId,
            null,
            false, "", "", "", "");
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<DeleteAuctionState, RequestAuctionDelete, TException> context, IBehavior<DeleteAuctionState, RequestAuctionDelete> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("scope");
    }
}