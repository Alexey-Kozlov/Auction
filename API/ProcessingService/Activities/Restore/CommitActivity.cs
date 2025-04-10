using Common.Contracts.Auction;
using Common.Contracts.Bid;
using Common.Contracts.EventSourcing;
using Common.Contracts.Finance;
using Common.Contracts.Image;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.RestoreStateMachine;

namespace ProcessingService.Activities.Restore;

public class CommitActivity : IStateMachineActivity<RestoreState, RestoreSnapShotESCommit>
{
    private readonly IPublishEndpoint _publishEndpoint;
    public CommitActivity(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }


    public async Task Execute(BehaviorContext<RestoreState, RestoreSnapShotESCommit> context, IBehavior<RestoreState, RestoreSnapShotESCommit> next)
    {
        await _publishEndpoint.Publish(new BidCommit
        {
            CorrelationId = context.Saga.CorrelationId,
            CallBackType = "Common.Contracts.EventSourcing.NotifyUIRestoreSnapShot",
            Commited = !context.Saga.IsError
        });
        await _publishEndpoint.Publish(new FinanceCommit
        {
            Commited = !context.Saga.IsError,
            CallBackType = "Common.Contracts.EventSourcing.NotifyUIRestoreSnapShot",
            CorrelationId = context.Saga.CorrelationId
        });
        await _publishEndpoint.Publish(new ImageCommit
        {
            Commited = !context.Saga.IsError,
            CallBackType = "Common.Contracts.EventSourcing.NotifyUIRestoreSnapShot",
            CorrelationId = context.Saga.CorrelationId
        });
        await _publishEndpoint.Publish(new AuctionCommit
        {
            Commited = !context.Saga.IsError,
            CallBackType = "Common.Contracts.EventSourcing.NotifyUIRestoreSnapShot",
            CorrelationId = context.Saga.CorrelationId
        });
        await _publishEndpoint.Publish(new NotificationCommit
        {
            Commited = !context.Saga.IsError,
            CallBackType = "Common.Contracts.EventSourcing.NotifyUIRestoreSnapShot",
            CorrelationId = context.Saga.CorrelationId
        });
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<RestoreState, RestoreSnapShotESCommit, TException> context, IBehavior<RestoreState, RestoreSnapShotESCommit> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}