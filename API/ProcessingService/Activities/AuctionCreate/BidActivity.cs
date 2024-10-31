using Common.Contracts;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.CreateAuctionStateMachine;

namespace ProcessingService.Activities.AuctionCreate;

public class BidActivity : IStateMachineActivity<CreateAuctionState, RequestAuctionCreate>
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

    public async Task Execute(BehaviorContext<CreateAuctionState, RequestAuctionCreate> context, IBehavior<CreateAuctionState, RequestAuctionCreate> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new AuctionCreatingBid(
                context.Saga.AuctionId,
                context.Saga.AuctionEnd,
                context.Saga.UserLogin,
                context.Saga.CorrelationId,
                context.Saga.ReservePrice
                ),
            nameof(AuctionCreatingBid),
            _config["ServicesName:BiddingService"],
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            OperationType.Insert,
            context.Saga.AuctionId);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<CreateAuctionState, RequestAuctionCreate, TException> context, IBehavior<CreateAuctionState, RequestAuctionCreate> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}