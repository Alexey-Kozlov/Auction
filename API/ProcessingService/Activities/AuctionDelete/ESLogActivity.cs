using Common.Contracts.Auction;
using Common.Contracts.Finance;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.DeleteAuctionStateMachine;

namespace ProcessingService.Activities.AuctionDelete;

public class ESLogActivity : IStateMachineActivity<DeleteAuctionState, RequestAuctionDelete>
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

    public async Task Execute(BehaviorContext<DeleteAuctionState, RequestAuctionDelete> context, IBehavior<DeleteAuctionState, RequestAuctionDelete> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new FinanceItem
            {
                ActionDate = DateTime.UtcNow,
                AuctionId = context.Message.AuctionId, //важно
                FinanceId = Guid.NewGuid(), //не важно
                Status = FinanceRecordStatus.Приход, //не важно
                UserLogin = context.Message.UserLogin //важно
            },
            nameof(FinanceItem),
            "Common.Contracts.Processing.ESLogAuctionDeleted",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            Command.AuctionDelete,
            "",
            context.Saga.AuctionId,
            false, "", "", "");
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<DeleteAuctionState, RequestAuctionDelete, TException> context, IBehavior<DeleteAuctionState, RequestAuctionDelete> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction");
    }
}