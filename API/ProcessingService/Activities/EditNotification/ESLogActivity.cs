using Common.Contracts.Notification;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.EditNotificationStateMachine;

namespace ProcessingService.Activities.EditNotification;

public class ESLogActivity : IStateMachineActivity<EditNotificationState, RequestEditNotification>
{
    private readonly SendEventToES _sendEventToES;
    public ESLogActivity(SendEventToES sendEventToES)
    {
        _sendEventToES = sendEventToES;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<EditNotificationState, RequestEditNotification> context, IBehavior<EditNotificationState, RequestEditNotification> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new RequestEditNotification
            {
                ItemId = context.Saga.ItemId.Value,
                Enable = context.Saga.Enable,
                CorrelationId = context.Saga.CorrelationId,
                SessionId = context.Saga.SessionId,
                UserLogin = context.Saga.UserLogin
            },
            nameof(RequestEditNotification),
            "Common.Contracts.Processing.ESLogEditNotification",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            Command.EditNotification,
            "",
            context.Saga.ItemId,
            context.Saga.ItemId,
            false, "", "", "");
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<EditNotificationState, RequestEditNotification, TException> context, IBehavior<EditNotificationState, RequestEditNotification> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction");
    }
}