using Common.Contracts.ELKSearch;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.ElkIndexStateMachine;

namespace ProcessingService.Activities.ElkIndex;

public class ESLogActivity : IStateMachineActivity<ElkIndexState, ElkIndexReset>
{
    private readonly SendEventToES _sendEventToES;
    public ESLogActivity(SendEventToES sendEventToES)
    {
        _sendEventToES = sendEventToES;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<ElkIndexState, ElkIndexReset> context, IBehavior<ElkIndexState, ElkIndexReset> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new RequestElkIndex
            {
                UserLogin = context.Saga.UserLogin,
                CorrelationId = context.Saga.CorrelationId,
            },
            nameof(RequestElkIndex),
            "Common.Contracts.Processing.ESLogElkIndex",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            Command.IndexELK,
            "",
            null,
            false, "", "", "");
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<ElkIndexState, ElkIndexReset, TException> context, IBehavior<ElkIndexState, ElkIndexReset> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-create");
    }
}