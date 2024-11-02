using Common.Contracts;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.DeleteAuctionStateMachine;

namespace ProcessingService.Activities.AuctionDelete;

public class FinanceActivity : IStateMachineActivity<DeleteAuctionState, RequestAuctionDelete>
{
    private readonly SendEventToES _sendEventToES;
    private readonly IConfiguration _config;
    public FinanceActivity(SendEventToES sendEventToES, IConfiguration config)
    {
        _sendEventToES = sendEventToES;
        _config = config;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<DeleteAuctionState, RequestAuctionDelete> context, IBehavior<DeleteAuctionState, RequestAuctionDelete> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new FinanceItem
            {
                ActionDate = DateTime.UtcNow,
                AuctionId = context.Message.AuctionId,
                Id = Guid.NewGuid(),
                Status = FinanceRecordStatus.Приход,
                UserLogin = context.Message.UserLogin
            },
            nameof(FinanceItem),
            _config["ServicesName:FinanceService"],
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            OperationType.Delete,
            context.Saga.AuctionId);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<DeleteAuctionState, RequestAuctionDelete, TException> context, IBehavior<DeleteAuctionState, RequestAuctionDelete> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}