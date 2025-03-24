using Common.Contracts.Auction;
using Common.Contracts.ELKSearch;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.ElkIndexStateMachine;

namespace ProcessingService.Activities.ElkIndex;

public class CommitActivity : IStateMachineActivity<ElkIndexState, ElkIndexESCommit>
{
    private readonly SendEventToES _sendEventToES;
    public CommitActivity(SendEventToES sendEventToES)
    {
        _sendEventToES = sendEventToES;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }


    public async Task Execute(BehaviorContext<ElkIndexState, ElkIndexESCommit> context, IBehavior<ElkIndexState, ElkIndexESCommit> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new RequestCommitESOperation(context.Saga.CorrelationId),
            nameof(CommitESOperation),
            "Common.Contracts.ELKSearch.ElkIndexEnd",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            Command.IndexELK,
            "",
            null,
            false);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<ElkIndexState, ElkIndexESCommit, TException> context, IBehavior<ElkIndexState, ElkIndexESCommit> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}