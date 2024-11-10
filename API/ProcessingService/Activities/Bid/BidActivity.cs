using Common.Contracts;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.BidPlacedStateMachine;

namespace ProcessingService.Activities.Bid;

public class BidActivity : IStateMachineActivity<BidPlacedState, BidFinanceGranted>
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

    public async Task Execute(BehaviorContext<BidPlacedState, BidFinanceGranted> context, IBehavior<BidPlacedState, BidFinanceGranted> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new BidItem
            {
                BidId = Guid.NewGuid(),
                AuctionId = context.Saga.AuctionId,
                Bidder = context.Saga.Bidder,
                BidTime = DateTime.UtcNow,
                Amount = context.Saga.Amount
            },
            nameof(BidItem),
            "Common.Contracts.BidPlaced",
            context.Message.CorrelationId,
            context.Saga.Bidder,
            OperationType.Bid,
            context.Saga.AuctionId);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<BidPlacedState, BidFinanceGranted, TException> context, IBehavior<BidPlacedState, BidFinanceGranted> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}