using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.RestoreStateMachine;

namespace ProcessingService.Activities.Restore;

public class ESLogActivityReset : IStateMachineActivity<RestoreState, RequestRestoreSnapShot>
{
    private readonly SendEventToES _sendEventToES;
    private readonly IConfiguration _config;
    public ESLogActivityReset(SendEventToES sendEventToES, IConfiguration config)
    {
        _sendEventToES = sendEventToES;
        _config = config;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<RestoreState, RequestRestoreSnapShot> context, IBehavior<RestoreState, RequestRestoreSnapShot> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new ESLog_ResetSnapShot
            {
                CorrelationId = context.Saga.CorrelationId
            },
            nameof(ESLog_ResetSnapShot),
            "Common.Contracts.Processing.ESLog_ResetSnapShot",
            context.Saga.CorrelationId,
            context.Saga.UserLogin,
            Command.RestoreSnapShot,
            null);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<RestoreState, RequestRestoreSnapShot, TException> context, IBehavior<RestoreState, RequestRestoreSnapShot> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction");
    }
}