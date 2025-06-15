using Common.Contracts.Auction;
using Common.Contracts.Bid;
using Common.Contracts.EventSourcing;
using Common.Contracts.Finance;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.BidPlacedStateMachine;

namespace ProcessingService.Activities.Bid;

public class CommitActivity : IStateMachineActivity<BidPlacedState, BidCreateESCommit>
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


    public async Task Execute(BehaviorContext<BidPlacedState, BidCreateESCommit> context, IBehavior<BidPlacedState, BidCreateESCommit> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new RequestCommitESOperation(context.Saga.CorrelationId),
            nameof(CommitESOperation),
            "Common.Contracts.Bid.BidNotificationEvent",
            context.Saga.CorrelationId,
            context.Saga.Bidder,
            Command.PlaceBid,
            "",
            context.Saga.AuctionId,
            context.Saga.ItemId,
            context.Saga.IsError,
            context.Message.ErrorMessage,
            context.Message.ErrorExceptionMessage,
            context.Message.ErrorServiceName);

        await _publishEndpoint.Publish(new FinanceCommit
        {
            Commited = !context.Saga.IsError,
            CallBackType = "Common.Contracts.Bid.BidNotificationEvent",
            CorrelationId = context.Message.CorrelationId,
            ErrorMessage = context.Message.ErrorMessage,
            ErrorExceptionMessage = context.Message.ErrorExceptionMessage,
            ErrorServiceName = context.Message.ErrorServiceName,
            UserLogin = context.Saga.Bidder
        });
        await _publishEndpoint.Publish(new BidCommit
        {
            Commited = !context.Saga.IsError,
            CallBackType = "Common.Contracts.Bid.BidNotificationEvent",
            CorrelationId = context.Message.CorrelationId,
            ErrorMessage = context.Message.ErrorMessage,
            ErrorExceptionMessage = context.Message.ErrorExceptionMessage,
            ErrorServiceName = context.Message.ErrorServiceName,
            UserLogin = context.Saga.Bidder
        });
        await _publishEndpoint.Publish(new AuctionCommit
        {
            Commited = !context.Saga.IsError,
            CallBackType = "Common.Contracts.Bid.BidNotificationEvent",
            CorrelationId = context.Message.CorrelationId,
            ErrorMessage = context.Message.ErrorMessage,
            ErrorExceptionMessage = context.Message.ErrorExceptionMessage,
            ErrorServiceName = context.Message.ErrorServiceName,
            UserLogin = context.Saga.Bidder
        });
        await _publishEndpoint.Publish(new NotificationCommit
        {
            Commited = !context.Saga.IsError,
            CallBackType = "Common.Contracts.Bid.BidNotificationEvent",
            CorrelationId = context.Message.CorrelationId,
            ErrorMessage = context.Message.ErrorMessage,
            ErrorExceptionMessage = context.Message.ErrorExceptionMessage,
            ErrorServiceName = context.Message.ErrorServiceName,
            UserLogin = context.Saga.Bidder
        });

        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<BidPlacedState, BidCreateESCommit, TException> context, IBehavior<BidPlacedState, BidCreateESCommit> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}