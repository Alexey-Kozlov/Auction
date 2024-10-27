using Common.Contracts;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.UpdateAuctionStateMachine;

namespace ProcessingService.Activities.AuctionUpdate;

public class BidActivity : IStateMachineActivity<UpdateAuctionState, RequestAuctionUpdate>
{
    private readonly SendEventToES _sendEventToES;
    private readonly IConfiguration _config;
    public BidActivity(SendEventToES sendEventToES, IConfiguration config)
    {
        _sendEventToES = sendEventToES;
        _config = config;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<UpdateAuctionState, RequestAuctionUpdate> context, IBehavior<UpdateAuctionState, RequestAuctionUpdate> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new AuctionUpdatingBid(
                context.Saga.AuctionId,
                context.Saga.AuctionEnd,
                context.Saga.CorrelationId
                ),
            nameof(AuctionUpdatingBid),
            _config["ServicesName:BiddingService"],
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            context.Saga.AuctionId);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<UpdateAuctionState, RequestAuctionUpdate, TException> context, IBehavior<UpdateAuctionState, RequestAuctionUpdate> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}