using Common.Contracts.Bid;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.BidPlacedStateMachine;

namespace ProcessingService.Activities.Bid;

public class ESLogActivity : IStateMachineActivity<BidPlacedState, RequestBidPlace>
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

    public async Task Execute(BehaviorContext<BidPlacedState, RequestBidPlace> context, IBehavior<BidPlacedState, RequestBidPlace> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new BidItem
            {
                ItemId = context.Saga.ItemId.Value,
                AuctionId = context.Saga.AuctionId,
                Bidder = context.Saga.Bidder,
                BidTime = DateTime.UtcNow,
                Amount = context.Saga.Amount,
                CorrelationId = context.Saga.CorrelationId
            },
            nameof(BidItem),
            "Common.Contracts.Processing.ESLogPlaceBid",
            context.Message.CorrelationId,
            context.Saga.Bidder,
            Command.PlaceBid,
            "",
            context.Saga.AuctionId,
            context.Saga.ItemId,
            false, "", "", "");
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<BidPlacedState, RequestBidPlace, TException> context, IBehavior<BidPlacedState, RequestBidPlace> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction");
    }
}