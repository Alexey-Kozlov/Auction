using Common.Contracts.EventSourcing;
using Common.Contracts.Finance;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.RestoreStateMachine;

namespace ProcessingService.Activities.Restore;

public class CommitActivity : IStateMachineActivity<RestoreState, RestoreSnapShotESCommit>
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


    public async Task Execute(BehaviorContext<RestoreState, RestoreSnapShotESCommit> context, IBehavior<RestoreState, RestoreSnapShotESCommit> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new RequestCommitESOperation(context.Saga.CorrelationId),
            nameof(CommitESOperation),
            "Common.Contracts.EventSourcing.RestoreSnapShotComplete",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            Command.RestoreSnapShot,
            null);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<RestoreState, RestoreSnapShotESCommit, TException> context, IBehavior<RestoreState, RestoreSnapShotESCommit> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}