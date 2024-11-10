using Common.Contracts;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.BidPlacedStateMachine;

namespace ProcessingService.Activities.Bid;

public class NotificationActivity : IStateMachineActivity<BidPlacedState, BidSearchPlaced>
{
    private readonly SendEventToES _sendEventToES;
    private readonly IConfiguration _config;
    public NotificationActivity(SendEventToES sendEventToES, IConfiguration config)
    {
        _sendEventToES = sendEventToES;
        _config = config;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<BidPlacedState, BidSearchPlaced> context, IBehavior<BidPlacedState, BidSearchPlaced> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new NotifyItem
            {
                AuctionId = context.Saga.AuctionId,
                UserLogin = context.Saga.Bidder
            },
            nameof(NotifyItem),
            "Common.Contracts.BidNotificationProcessed",
            context.Message.CorrelationId,
            context.Saga.Bidder,
            OperationType.Bid,
            context.Saga.AuctionId);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<BidPlacedState, BidSearchPlaced, TException> context, IBehavior<BidPlacedState, BidSearchPlaced> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-create");
    }
}