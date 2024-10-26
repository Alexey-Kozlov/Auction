using System.Reflection;
using Common.Contracts;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.CreateAuctionStateMachine;

namespace ProcessingService.Activities;

public class CommitCreatingAuctionActivity : IStateMachineActivity<CreateAuctionState, AuctionCreatedElk>
{
    private readonly SendEventToES _sendEventToES;
    public CommitCreatingAuctionActivity(SendEventToES sendEventToES)
    {
        _sendEventToES = sendEventToES;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }


    public async Task Execute(BehaviorContext<CreateAuctionState, AuctionCreatedElk> context, IBehavior<CreateAuctionState, AuctionCreatedElk> next)
    {
        await _sendEventToES.SendItemToEventSourcing(context.Message, nameof(CommitESOperation),
            Assembly.GetExecutingAssembly().GetName().Name, context.Message.CorrelationId);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<CreateAuctionState, AuctionCreatedElk, TException> context, IBehavior<CreateAuctionState, AuctionCreatedElk> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-create2");
    }
}