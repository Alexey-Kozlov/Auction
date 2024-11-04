using Common.Contracts;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.DeleteAuctionStateMachine;

namespace ProcessingService.Activities.AuctionDelete;

public class SearchActivity : IStateMachineActivity<DeleteAuctionState, AuctionDeletedImage>
{
    private readonly SendEventToES _sendEventToES;
    private readonly IConfiguration _config;
    public SearchActivity(SendEventToES sendEventToES, IConfiguration config)
    {
        _sendEventToES = sendEventToES;
        _config = config;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<DeleteAuctionState, AuctionDeletedImage> context, IBehavior<DeleteAuctionState, AuctionDeletedImage> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new AuctionItem
            {
                AuctionId = context.Saga.AuctionId,
                Seller = context.Saga.UserLogin
            },
            nameof(AuctionItem),
            "Common.Contracts.AuctionDeletedSearch",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            OperationType.Delete,
            context.Saga.AuctionId);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<DeleteAuctionState, AuctionDeletedImage, TException> context, IBehavior<DeleteAuctionState, AuctionDeletedImage> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}