using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.RestoreStateMachine;

namespace ProcessingService.Activities.Restore;

public class ESLogActivityGetRecords : IStateMachineActivity<RestoreState, ResetItems>
{
    private readonly SendEventToES _sendEventToES;
    public ESLogActivityGetRecords(SendEventToES sendEventToES)
    {
        _sendEventToES = sendEventToES;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<RestoreState, ResetItems> context, IBehavior<RestoreState, ResetItems> next)
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
            "Common.Contracts.Processing.ESLogRestoreImages",
            context.Saga.CorrelationId,
            context.Saga.UserLogin,
            Command.RestoreSnapShot,
            "",
            null,
            null,
            context.Saga.IsError,
            context.Message.ErrorMessage,
            context.Message.ErrorExceptionStack,
            context.Message.ErrorExceptionInputData,
            context.Message.ErrorServiceName);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<RestoreState, ResetItems, TException> context, IBehavior<RestoreState, ResetItems> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("scope");
    }
}