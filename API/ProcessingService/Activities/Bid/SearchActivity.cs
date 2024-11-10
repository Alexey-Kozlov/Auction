using Common.Contracts;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.BidPlacedStateMachine;

namespace ProcessingService.Activities.Bid;

public class SearchActivity : IStateMachineActivity<BidPlacedState, BidPlaced>
{
    private readonly SendEventToES _sendEventToES;
    private readonly IConfiguration _config;
    public SearchActivity(SendEventToES sendEventToES, IConfiguration config)
    {
        _sendEventToES = sendEventToES;
        _config = config;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<BidPlacedState, BidPlaced> context, IBehavior<BidPlacedState, BidPlaced> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new AuctionItem
            {
                AuctionId = context.Saga.AuctionId,
                CurrentHighBid = context.Saga.Amount,
                Seller = context.Saga.Bidder,
                UpdatedAt = DateTime.UtcNow
            },
            nameof(AuctionItem),
            "Common.Contracts.BidSearchPlaced",
            context.Message.CorrelationId,
            context.Saga.Bidder,
            OperationType.Bid,
            context.Saga.AuctionId);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<BidPlacedState, BidPlaced, TException> context, IBehavior<BidPlacedState, BidPlaced> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-create");
    }
}