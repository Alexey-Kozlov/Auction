using Common.Contracts.EventSourcing;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.EditNotificationStateMachine;

namespace ProcessingService.Activities.EditNotification;

public class CommitActivity : IStateMachineActivity<EditNotificationState, EditNotificationESCommit>
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


    public async Task Execute(BehaviorContext<EditNotificationState, EditNotificationESCommit> context, IBehavior<EditNotificationState, EditNotificationESCommit> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new RequestCommitESOperation(context.Saga.CorrelationId),
            nameof(CommitESOperation),
            "Common.Contracts.Notification.EditNotificationEvent",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            Command.EditNotification,
            "",
            null,
            context.Saga.ItemId,
            context.Saga.IsError,
            context.Message.ErrorMessage,
            context.Message.ErrorExceptionStack,
            context.Message.ErrorExceptionInputData,
            context.Message.ErrorServiceName);

        await _publishEndpoint.Publish(new NotificationCommit
        {
            Commited = !context.Saga.IsError,
            CallBackType = "Common.Contracts.Notification.EditNotificationEvent",
            CorrelationId = context.Message.CorrelationId,
            ErrorMessage = context.Message.ErrorMessage,
            ErrorExceptionStack = context.Message.ErrorExceptionStack,
            ErrorExceptionInputData = context.Message.ErrorExceptionInputData,
            ErrorServiceName = context.Message.ErrorServiceName,
            UserLogin = context.Saga.UserLogin
        });
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<EditNotificationState, EditNotificationESCommit, TException> context, IBehavior<EditNotificationState, EditNotificationESCommit> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("scope");
    }
}