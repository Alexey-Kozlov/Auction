using Common.Contracts.Auction;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.DeleteAuctionStateMachine;

namespace ProcessingService.Activities.AuctionDelete;

public class NotificationActivity : IStateMachineActivity<DeleteAuctionState, AuctionDeletedElk>
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

    public async Task Execute(BehaviorContext<DeleteAuctionState, AuctionDeletedElk> context, IBehavior<DeleteAuctionState, AuctionDeletedElk> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new NotifyItem
            {
                AuctionId = context.Saga.AuctionId,
                UserLogin = context.Saga.UserLogin
            },
            nameof(NotifyItem),
            "Common.Contracts.AuctionDeletedNotification",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            OperationType.AuctionDelete,
            context.Saga.AuctionId);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<DeleteAuctionState, AuctionDeletedElk, TException> context, IBehavior<DeleteAuctionState, AuctionDeletedElk> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-create");
    }
}