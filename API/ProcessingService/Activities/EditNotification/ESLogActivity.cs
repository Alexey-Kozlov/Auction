using Common.Contracts.Notification;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.EditNotificationStateMachine;

namespace ProcessingService.Activities.EditNotification;

public class ESLogActivity : IStateMachineActivity<EditNotificationState, RequestEditNotification>
{
    private readonly SendEventToES _sendEventToES;
    private readonly IConfiguration _config;
    public ESLogActivity(SendEventToES sendEventToES, IConfiguration config)
    {
        _sendEventToES = sendEventToES;
        _config = config;
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
                AuctionId = context.Saga.AuctionId,
                Enable = context.Saga.Enable,
                CorrelationId = context.Saga.CorrelationId,
                SessionId = context.Saga.SessionId,
                UserLogin = context.Saga.UserLogin
            },
            nameof(RequestEditNotification),
            "Common.Contracts.Processing.ESLog_EditNotification",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            Command.EditNotification,
            context.Saga.AuctionId);
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