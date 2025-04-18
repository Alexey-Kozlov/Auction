using Common.Contracts.Auction;
using Common.Contracts.ELKSearch;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.FinishAuctionStateMachine;



namespace ProcessingService.Activities.AuctionFinish;

public class CommitActivity : IStateMachineActivity<FinishAuctionState, AuctionFinishedCommit>
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


    public async Task Execute(BehaviorContext<FinishAuctionState, AuctionFinishedCommit> context, IBehavior<FinishAuctionState, AuctionFinishedCommit> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new RequestCommitESOperation(context.Saga.CorrelationId),
            nameof(CommitESOperation),
            "Common.Contracts.Auction.AuctionFinishedNotification",
            context.Message.CorrelationId,
            "",
            Command.AuctionFinished,
            "",
            null,
            context.Saga.IsError,
            context.Message.ErrorMessage,
            context.Message.ErrorExceptionMessage,
            context.Message.ErrorServiceName);

        await _publishEndpoint.Publish(new AuctionCommit
        {
            Commited = !context.Saga.IsError,
            CallBackType = "Common.Contracts.Auction.AuctionFinishedNotification",
            CorrelationId = context.Saga.CorrelationId,
            ErrorMessage = context.Message.ErrorMessage,
            ErrorExceptionMessage = context.Message.ErrorExceptionMessage,
            ErrorServiceName = context.Message.ErrorServiceName,
            UserLogin = "SystemService"
        });

        await _publishEndpoint.Publish(new ElkCommit
        {
            Commited = !context.Saga.IsError,
            CallBackType = "Common.Contracts.Auction.AuctionFinishedNotification",
            CorrelationId = context.Saga.CorrelationId,
            ErrorMessage = context.Message.ErrorMessage,
            ErrorExceptionMessage = context.Message.ErrorExceptionMessage,
            ErrorServiceName = context.Message.ErrorServiceName,
            UserLogin = "SystemService"
        });
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<FinishAuctionState, AuctionFinishedCommit, TException> context, IBehavior<FinishAuctionState, AuctionFinishedCommit> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}