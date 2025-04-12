using Common.Contracts.EventSourcing;
using Common.Contracts.Finance;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.FinanceStateMachine;

namespace ProcessingService.Activities.Finance;

public class CommitActivity : IStateMachineActivity<FinanceState, FinanceCreateESCommit>
{
    private readonly SendEventToES _sendEventToES;
    private readonly IPublishEndpoint _publishEndpoint;
    public CommitActivity(SendEventToES sendEventToES, IPublishEndpoint publishEndpoint)
    {
        _sendEventToES = sendEventToES;
        _publishEndpoint = publishEndpoint;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }


    public async Task Execute(BehaviorContext<FinanceState, FinanceCreateESCommit> context, IBehavior<FinanceState, FinanceCreateESCommit> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new RequestCommitESOperation(context.Saga.CorrelationId),
            nameof(CommitESOperation),
            "Common.Contracts.Finance.FinanceNotificationCreated",
            context.Saga.CorrelationId,
            context.Saga.UserLogin,
            Command.FinanceCreate,
            "",
            null,
            context.Saga.IsError,
            context.Message.Message,
            context.Message.ExceptionMessage,
            context.Message.ServiceName);
        await _publishEndpoint.Publish(new FinanceCommit
        {
            Commited = !context.Saga.IsError,
            CallBackType = "Common.Contracts.Finance.FinanceNotificationCreated",
            CorrelationId = context.Saga.CorrelationId,
            Message = context.Message.Message,
            ExceptionMessage = context.Message.ExceptionMessage,
            ServiceName = context.Message.ServiceName,
            UserLogin = context.Message.UserLogin
        });
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<FinanceState, FinanceCreateESCommit, TException> context, IBehavior<FinanceState, FinanceCreateESCommit> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}