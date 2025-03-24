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
    private readonly IConfiguration _config;
    public CommitActivity(SendEventToES sendEventToES, IConfiguration config)
    {
        _sendEventToES = sendEventToES;
        _config = config;
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
            "Common.Contracts.Notification.EditNotificationComplete",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            Command.EditNotification,
            "",
            null,
            !context.Message.IsError);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<EditNotificationState, EditNotificationESCommit, TException> context, IBehavior<EditNotificationState, EditNotificationESCommit> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}