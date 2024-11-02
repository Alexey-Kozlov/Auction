using Common.Contracts;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.UpdateAuctionStateMachine;

namespace ProcessingService.Activities.AuctionUpdate;

public class SearchActivity : IStateMachineActivity<UpdateAuctionState, AuctionUpdatedImage>
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

    public async Task Execute(BehaviorContext<UpdateAuctionState, AuctionUpdatedImage> context, IBehavior<UpdateAuctionState, AuctionUpdatedImage> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new AuctionItem
            {
                AuctionEnd = context.Saga.AuctionEnd,
                AuctionId = context.Saga.AuctionId,
                Title = context.Saga.Title,
                Properties = context.Saga.Properties,
                Description = context.Saga.Description,
                Seller = context.Saga.UserLogin
            },
            nameof(AuctionItem),
            "Common.Contracts.AuctionUpdatedSearch",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            OperationType.Update,
            context.Saga.AuctionId);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<UpdateAuctionState, AuctionUpdatedImage, TException> context, IBehavior<UpdateAuctionState, AuctionUpdatedImage> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}