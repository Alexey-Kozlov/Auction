using Common.Contracts.Bid;
using Common.Contracts.Finance;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.BidPlacedStateMachine;

namespace ProcessingService.Activities.Bid;

public class FinanceActivity : IStateMachineActivity<BidPlacedState, RequestBidPlace>
{
    private readonly SendEventToES _sendEventToES;
    private readonly IConfiguration _config;
    public FinanceActivity(SendEventToES sendEventToES, IConfiguration config)
    {
        _sendEventToES = sendEventToES;
        _config = config;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<BidPlacedState, RequestBidPlace> context, IBehavior<BidPlacedState, RequestBidPlace> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new FinanceItem
            {
                ActionDate = DateTime.UtcNow,
                FinanceId = Guid.NewGuid(),
                AuctionId = context.Saga.AuctionId,
                Status = FinanceRecordStatus.Расход,
                UserLogin = context.Saga.Bidder,
                Value = context.Saga.Amount
            },
            nameof(FinanceItem),
            "Common.Contracts.BidFinanceGranted",
            context.Message.CorrelationId,
            context.Saga.Bidder,
            Command.PlaceBid,
            context.Saga.AuctionId);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<BidPlacedState, RequestBidPlace, TException> context, IBehavior<BidPlacedState, RequestBidPlace> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}