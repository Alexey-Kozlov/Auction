using Common.Contracts.Auction;
using Common.Contracts.Bid;
using Common.Contracts.Communication;
using Common.Contracts.EventSourcing;
using Common.Contracts.Finance;
using Common.Contracts.Image;
using Common.Contracts.Notification;
using Common.Contracts.Tag;
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
            CallBackType = "Common.Contracts.EventSourcing.SendStartFinishService",
            Commited = !context.Saga.IsError,
            ErrorMessage = context.Message.ErrorMessage,
            ErrorExceptionStack = context.Message.ErrorExceptionStack,
            ErrorExceptionInputData = context.Message.ErrorExceptionInputData,
            ErrorServiceName = context.Message.ErrorServiceName,
            UserLogin = context.Saga.UserLogin
        });
        await _publishEndpoint.Publish(new FinanceCommit
        {
            Commited = !context.Saga.IsError,
            CallBackType = "Common.Contracts.EventSourcing.SendStartFinishService",
            CorrelationId = context.Saga.CorrelationId,
            ErrorMessage = context.Message.ErrorMessage,
            ErrorExceptionStack = context.Message.ErrorExceptionStack,
            ErrorExceptionInputData = context.Message.ErrorExceptionInputData,
            ErrorServiceName = context.Message.ErrorServiceName,
            UserLogin = context.Saga.UserLogin
        });
        await _publishEndpoint.Publish(new ImageCommit
        {
            Commited = !context.Saga.IsError,
            CallBackType = "Common.Contracts.EventSourcing.SendStartFinishService",
            CorrelationId = context.Saga.CorrelationId,
            ErrorMessage = context.Message.ErrorMessage,
            ErrorExceptionStack = context.Message.ErrorExceptionStack,
            ErrorExceptionInputData = context.Message.ErrorExceptionInputData,
            ErrorServiceName = context.Message.ErrorServiceName,
            UserLogin = context.Saga.UserLogin
        });
        await _publishEndpoint.Publish(new AuctionCommit
        {
            Commited = !context.Saga.IsError,
            CallBackType = "Common.Contracts.EventSourcing.SendStartFinishService",
            CorrelationId = context.Saga.CorrelationId,
            ErrorMessage = context.Message.ErrorMessage,
            ErrorExceptionStack = context.Message.ErrorExceptionStack,
            ErrorExceptionInputData = context.Message.ErrorExceptionInputData,
            ErrorServiceName = context.Message.ErrorServiceName,
            UserLogin = context.Saga.UserLogin
        });
        await _publishEndpoint.Publish(new NotificationCommit
        {
            Commited = !context.Saga.IsError,
            CallBackType = "Common.Contracts.EventSourcing.SendStartFinishService",
            CorrelationId = context.Saga.CorrelationId,
            ErrorMessage = context.Message.ErrorMessage,
            ErrorExceptionStack = context.Message.ErrorExceptionStack,
            ErrorExceptionInputData = context.Message.ErrorExceptionInputData,
            ErrorServiceName = context.Message.ErrorServiceName,
            UserLogin = context.Saga.UserLogin
        });
        await _publishEndpoint.Publish(new CommunicationCommit
        {
            Commited = !context.Saga.IsError,
            CallBackType = "Common.Contracts.EventSourcing.SendStartFinishService",
            CorrelationId = context.Saga.CorrelationId,
            ErrorMessage = context.Message.ErrorMessage,
            ErrorExceptionStack = context.Message.ErrorExceptionStack,
            ErrorExceptionInputData = context.Message.ErrorExceptionInputData,
            ErrorServiceName = context.Message.ErrorServiceName,
            UserLogin = context.Saga.UserLogin
        });
        await _publishEndpoint.Publish(new TagCommit
        {
            Commited = !context.Saga.IsError,
            CallBackType = "Common.Contracts.EventSourcing.SendStartFinishService",
            CorrelationId = context.Saga.CorrelationId,
            ErrorMessage = context.Message.ErrorMessage,
            ErrorExceptionStack = context.Message.ErrorExceptionStack,
            ErrorExceptionInputData = context.Message.ErrorExceptionInputData,
            ErrorServiceName = context.Message.ErrorServiceName,
            UserLogin = context.Saga.UserLogin
        });
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<RestoreState, RestoreSnapShotESCommit, TException> context, IBehavior<RestoreState, RestoreSnapShotESCommit> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("scope");
    }
}