using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.RestoreStateMachine;

namespace ProcessingService.Activities.Restore;

public class ESLogActivityReset : IStateMachineActivity<RestoreState, RequestRestoreItems>
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

    public async Task Execute(BehaviorContext<RestoreState, RequestRestoreItems> context, IBehavior<RestoreState, RequestRestoreItems> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new ESLogResetSnapShot
            {
                CorrelationId = context.Saga.CorrelationId
            },
            nameof(ESLogResetSnapShot),
            "Common.Contracts.Processing.ESLogRestoreItems",
            context.Saga.CorrelationId,
            context.Saga.UserLogin,
            Command.RestoreSnapShot,
            "",
            null);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<RestoreState, RequestRestoreItems, TException> context, IBehavior<RestoreState, RequestRestoreItems> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction");
    }
}