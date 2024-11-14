using Common.Contracts.Auction;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.UpdateAuctionStateMachine;

namespace ProcessingService.Activities.AuctionUpdate;

public class NotificationActivity : IStateMachineActivity<UpdateAuctionState, AuctionUpdatedSearch>
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

    public async Task Execute(BehaviorContext<UpdateAuctionState, AuctionUpdatedSearch> context, IBehavior<UpdateAuctionState, AuctionUpdatedSearch> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new NotifyItem
            {
                AuctionId = context.Saga.AuctionId,
                UserLogin = context.Saga.UserLogin
            },
            nameof(NotifyItem),
            "Common.Contracts.AuctionUpdatedNotification",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            Command.AuctionUpdate,
            context.Saga.AuctionId);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<UpdateAuctionState, AuctionUpdatedSearch, TException> context, IBehavior<UpdateAuctionState, AuctionUpdatedSearch> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-create");
    }
}