using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.RestoreStateMachine;

namespace ProcessingService.Activities.Restore;

public class ESLogActivityGetRecords : IStateMachineActivity<RestoreState, ESLog_RestoreItems>
{
    private readonly SendEventToES _sendEventToES;
    private readonly IConfiguration _config;
    public ESLogActivityGetRecords(SendEventToES sendEventToES, IConfiguration config)
    {
        _sendEventToES = sendEventToES;
        _config = config;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<RestoreState, ESLog_RestoreItems> context, IBehavior<RestoreState, ESLog_RestoreItems> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new RequestRestoreItems
            {
                RestoreDate = context.Saga.RestoreDate,
                UserLogin = context.Saga.UserLogin,
                CorrelationId = context.Saga.CorrelationId,
                ResetLog = context.Saga.ResetLog
            },
            nameof(RequestRestoreItems),
            "Common.Contracts.Processing.ESLog_RestoreImages",
            context.Saga.CorrelationId,
            context.Saga.UserLogin,
            Command.RestoreSnapShot,
            "",
            null);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<RestoreState, ESLog_RestoreItems, TException> context, IBehavior<RestoreState, ESLog_RestoreItems> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction");
    }
}