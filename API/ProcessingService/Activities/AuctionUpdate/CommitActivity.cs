using Common.Contracts.Auction;
using Common.Contracts.ELKSearch;
using Common.Contracts.EventSourcing;
using Common.Contracts.Image;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.UpdateAuctionStateMachine;


namespace ProcessingService.Activities.AuctionUpdate;

public class CommitActivity : IStateMachineActivity<UpdateAuctionState, AuctionUpdateESCommit>
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


    public async Task Execute(BehaviorContext<UpdateAuctionState, AuctionUpdateESCommit> context, IBehavior<UpdateAuctionState, AuctionUpdateESCommit> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new RequestCommitESOperation(context.Saga.CorrelationId),
            nameof(CommitESOperation),
            "Common.Contracts.Auction.AuctionUpdatedNotificationEvent",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            Command.AuctionUpdate,
            "",
            context.Saga.AuctionId,
            context.Saga.IsError,
            context.Message.ErrorMessage,
            context.Message.ErrorExceptionMessage,
            context.Message.ErrorServiceName);
        await _publishEndpoint.Publish(new ImageCommit
        {
            Commited = !context.Saga.IsError,
            CallBackType = "Common.Contracts.Auction.AuctionUpdatedNotificationEvent",
            CorrelationId = context.Saga.CorrelationId,
            ErrorMessage = context.Message.ErrorMessage,
            ErrorExceptionMessage = context.Message.ErrorExceptionMessage,
            ErrorServiceName = context.Message.ErrorServiceName,
            UserLogin = context.Saga.UserLogin
        });
        await _publishEndpoint.Publish(new AuctionCommit
        {
            Commited = !context.Saga.IsError,
            CallBackType = "Common.Contracts.Auction.AuctionUpdatedNotificationEvent",
            CorrelationId = context.Saga.CorrelationId,
            ErrorMessage = context.Message.ErrorMessage,
            ErrorExceptionMessage = context.Message.ErrorExceptionMessage,
            ErrorServiceName = context.Message.ErrorServiceName,
            UserLogin = context.Saga.UserLogin
        });
        await _publishEndpoint.Publish(new ElkCommit
        {
            Commited = !context.Saga.IsError,
            CallBackType = "Common.Contracts.Auction.AuctionUpdatedNotificationEvent",
            CorrelationId = context.Saga.CorrelationId,
            ErrorMessage = context.Message.ErrorMessage,
            ErrorExceptionMessage = context.Message.ErrorExceptionMessage,
            ErrorServiceName = context.Message.ErrorServiceName,
            UserLogin = context.Saga.UserLogin,
            ElkIndex = "search_index"
        });
        await _publishEndpoint.Publish(new NotificationCommit
        {
            Commited = !context.Saga.IsError,
            CallBackType = "Common.Contracts.Auction.AuctionUpdatedNotificationEvent",
            CorrelationId = context.Saga.CorrelationId,
            ErrorMessage = context.Message.ErrorMessage,
            ErrorExceptionMessage = context.Message.ErrorExceptionMessage,
            ErrorServiceName = context.Message.ErrorServiceName,
            UserLogin = context.Saga.UserLogin
        });
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<UpdateAuctionState, AuctionUpdateESCommit, TException> context, IBehavior<UpdateAuctionState, AuctionUpdateESCommit> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}