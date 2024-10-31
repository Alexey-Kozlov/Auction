using Common.Contracts;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.FinanceStateMachine;

namespace ProcessingService.Activities.Finance;

public class CommitActivity : IStateMachineActivity<FinanceState, FinanceNotificationCreated>
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


    public async Task Execute(BehaviorContext<FinanceState, FinanceNotificationCreated> context, IBehavior<FinanceState, FinanceNotificationCreated> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new RequestCommitESOperation(context.Saga.CorrelationId),
            nameof(CommitESFinanceOperation),
            _config["ServicesName:ProcessingService"],
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            OperationType.Insert,
            null);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<FinanceState, FinanceNotificationCreated, TException> context, IBehavior<FinanceState, FinanceNotificationCreated> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}