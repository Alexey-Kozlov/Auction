using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.RestoreStateMachine;

namespace ProcessingService.Activities.Restore;

public class ESLogActivityGetImages : IStateMachineActivity<RestoreState, ESLogRestoreImages>
{
    private readonly SendEventToES _sendEventToES;
    private readonly IConfiguration _config;
    public ESLogActivityGetImages(SendEventToES sendEventToES, IConfiguration config)
    {
        _sendEventToES = sendEventToES;
        _config = config;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<RestoreState, ESLogRestoreImages> context, IBehavior<RestoreState, ESLogRestoreImages> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new RequestRestoreImages
            {
                RestoreDate = context.Saga.RestoreDate,
                UserLogin = context.Saga.UserLogin,
                CorrelationId = context.Saga.CorrelationId,
                MaxMessageSizeMb = 0,
                StartNumber = 0
            },
            nameof(RequestRestoreImages),
            "Common.Contracts.Processing.ESLogRestoreImages",
            context.Saga.CorrelationId,
            context.Saga.UserLogin,
            Command.RestoreSnapShot,
            "",
            null,
            false);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<RestoreState, ESLogRestoreImages, TException> context, IBehavior<RestoreState, ESLogRestoreImages> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction");
    }
}