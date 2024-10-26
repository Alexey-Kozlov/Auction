using System.Reflection;
using Common.Contracts;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.DeleteAuctionStateMachine;

namespace ProcessingService.Activities;

public class CommitDeletingAuctionActivity : IStateMachineActivity<DeleteAuctionState, AuctionDeletedElk>
{
    private readonly SendEventToES _sendEventToES;
    public CommitDeletingAuctionActivity(SendEventToES sendEventToES)
    {
        _sendEventToES = sendEventToES;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }


    public async Task Execute(BehaviorContext<DeleteAuctionState, AuctionDeletedElk> context, IBehavior<DeleteAuctionState, AuctionDeletedElk> next)
    {
        await _sendEventToES.SendItemToEventSourcing(context.Message, nameof(CommitESOperation),
            Assembly.GetExecutingAssembly().GetName().Name, context.Message.CorrelationId);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<DeleteAuctionState, AuctionDeletedElk, TException> context, IBehavior<DeleteAuctionState, AuctionDeletedElk> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-delete");
    }
}