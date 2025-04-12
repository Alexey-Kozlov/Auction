using Common.Contracts.Finance;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.FinanceStateMachine;

namespace ProcessingService.Activities.Finance;

public class ESLogActivity : IStateMachineActivity<FinanceState, RequestCreateFinance>
{
    private readonly SendEventToES _sendEventToES;
    private readonly IConfiguration _config;
    public ESLogActivity(SendEventToES sendEventToES, IConfiguration config)
    {
        _sendEventToES = sendEventToES;
        _config = config;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<FinanceState, RequestCreateFinance> context, IBehavior<FinanceState, RequestCreateFinance> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new FinanceItem
            {
                ActionDate = DateTime.UtcNow,
                FinanceId = Guid.NewGuid(),
                Status = FinanceRecordStatus.Приход,
                UserLogin = context.Message.UserLogin,
                Value = context.Message.Amount
            },
            nameof(FinanceItem),
            "Common.Contracts.Processing.ESLogFinanceCreated",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            Command.FinanceCreate,
            "",
            null,
            false, "", "", "");
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<FinanceState, RequestCreateFinance, TException> context, IBehavior<FinanceState, RequestCreateFinance> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction");
    }
}