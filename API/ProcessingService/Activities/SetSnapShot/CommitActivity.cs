using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.SetSnapShotStateMachine;

namespace ProcessingService.Activities.SetSnapShot;

public class CommitActivity : IStateMachineActivity<SetSnapShotState, SetSnapShotESCommit>
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


    public async Task Execute(BehaviorContext<SetSnapShotState, SetSnapShotESCommit> context, IBehavior<SetSnapShotState, SetSnapShotESCommit> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new RequestCommitESOperation(context.Saga.CorrelationId),
            nameof(CommitESOperation),
            "Common.Contracts.EventSourcing.NotifyUISetSnapShot",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            Command.RestoreSnapShot,
            "",
            null,
            context.Saga.IsError);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<SetSnapShotState, SetSnapShotESCommit, TException> context, IBehavior<SetSnapShotState, SetSnapShotESCommit> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}