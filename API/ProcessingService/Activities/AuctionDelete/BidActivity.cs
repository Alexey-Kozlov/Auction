using Common.Contracts;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.DeleteAuctionStateMachine;

namespace ProcessingService.Activities.AuctionDelete;

public class BidActivity : IStateMachineActivity<DeleteAuctionState, AuctionDeletedFinance>
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

    public async Task Execute(BehaviorContext<DeleteAuctionState, AuctionDeletedFinance> context, IBehavior<DeleteAuctionState, AuctionDeletedFinance> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new AuctionDeletingBid(
                context.Saga.AuctionId,
                context.Saga.CorrelationId,
                context.Saga.UserLogin
                ),
            nameof(AuctionDeletingBid),
            _config["ServicesName:BiddingService"],
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            OperationType.Delete,
            context.Saga.AuctionId);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<DeleteAuctionState, AuctionDeletedFinance, TException> context, IBehavior<DeleteAuctionState, AuctionDeletedFinance> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}