using Common.Contracts.Auction;
using Common.Contracts.Bid;
using Common.Contracts.ELKSearch;
using Common.Contracts.EventSourcing;
using Common.Contracts.Finance;
using Common.Contracts.Image;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.DeleteAuctionStateMachine;

namespace ProcessingService.Activities.AuctionDelete;

public class CommitActivity : IStateMachineActivity<DeleteAuctionState, AuctionDeleteESCommit>
{
    private readonly SendEventToES _sendEventToES;
    private readonly IPublishEndpoint _publishEndpoint;
    public CommitActivity(SendEventToES sendEventToES, IPublishEndpoint publishEndpoint)
    {
        _sendEventToES = sendEventToES;
        _publishEndpoint = publishEndpoint;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }


    public async Task Execute(BehaviorContext<DeleteAuctionState, AuctionDeleteESCommit> context, IBehavior<DeleteAuctionState, AuctionDeleteESCommit> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new RequestCommitESOperation(context.Saga.CorrelationId),
            nameof(CommitESOperation),
            "Common.Contracts.Auction.AuctionDeletedNotificationEvent",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            Command.AuctionDelete,
            "",
            context.Saga.AuctionId,
            context.Saga.IsError);
        await _publishEndpoint.Publish(new FinanceCommit
        {
            Commited = !context.Saga.IsError,
            CorrelationId = context.Saga.CorrelationId
        });
        await _publishEndpoint.Publish(new BidCommit
        {
            Commited = !context.Saga.IsError,
            CorrelationId = context.Saga.CorrelationId
        });
        await _publishEndpoint.Publish(new ImageCommit
        {
            Commited = !context.Saga.IsError,
            CorrelationId = context.Saga.CorrelationId
        });
        await _publishEndpoint.Publish(new AuctionCommit
        {
            Commited = !context.Saga.IsError,
            CorrelationId = context.Saga.CorrelationId
        });
        await _publishEndpoint.Publish(new ElkCommit
        {
            Commited = !context.Saga.IsError,
            CorrelationId = context.Saga.CorrelationId
        });
        await _publishEndpoint.Publish(new NotificationCommit
        {
            Commited = !context.Saga.IsError,
            CorrelationId = context.Saga.CorrelationId
        });
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<DeleteAuctionState, AuctionDeleteESCommit, TException> context, IBehavior<DeleteAuctionState, AuctionDeleteESCommit> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}