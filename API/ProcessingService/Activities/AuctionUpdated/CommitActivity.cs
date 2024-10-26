using System.Reflection;
using Common.Contracts;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.UpdateAuctionStateMachine;

namespace ProcessingService.Activities.AuctionUpdated;

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
        await _sendEventToES.SendItemToEventSourcing(context.Message, nameof(CommitESOperation),
             _config["ServicesName:ProcessingService"], context.Message.CorrelationId);
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