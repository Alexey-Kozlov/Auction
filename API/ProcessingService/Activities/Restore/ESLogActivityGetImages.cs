using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.RestoreStateMachine;

namespace ProcessingService.Activities.Restore;

public class ESLogActivityGetImages : IStateMachineActivity<RestoreState, ESLog_RestoreImages>
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

    public async Task Execute(BehaviorContext<RestoreState, ESLog_RestoreImages> context, IBehavior<RestoreState, ESLog_RestoreImages> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new RequestRestoreImages
            {
                RestoreDate = context.Saga.RestoreDate,
                UserLogin = context.Saga.UserLogin,
                CorrelationId = context.Saga.CorrelationId,
                MaxMessageSizeMb = int.Parse(_config["MaxMessageSizeMb"]),
                StartNumber = 0
            },
            nameof(RequestRestoreImages),
            "Common.Contracts.Processing.ESLog_RestoreImages",
            context.Saga.CorrelationId,
            context.Saga.UserLogin,
            Command.RestoreSnapShot,
            "",
            null);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<RestoreState, ESLog_RestoreImages, TException> context, IBehavior<RestoreState, ESLog_RestoreImages> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction");
    }
}