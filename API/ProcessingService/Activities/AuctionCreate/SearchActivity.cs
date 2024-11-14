using Common.Contracts.Auction;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.CreateAuctionStateMachine;

namespace ProcessingService.Activities.AuctionCreate;

public class SearchActivity : IStateMachineActivity<CreateAuctionState, AuctionCreatedImage>
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

    public async Task Execute(BehaviorContext<CreateAuctionState, AuctionCreatedImage> context, IBehavior<CreateAuctionState, AuctionCreatedImage> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new AuctionItem
            {
                AuctionId = context.Saga.AuctionId,
                Title = context.Saga.Title,
                Properties = context.Saga.Properties,
                Description = context.Saga.Description,
                Seller = context.Saga.UserLogin,
                AuctionEnd = context.Saga.AuctionEnd,
                ReservePrice = context.Saga.ReservePrice,
                CreateAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            nameof(AuctionItem),
            "Common.Contracts.AuctionCreatedSearch",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            Command.AuctionCreate,
            context.Saga.AuctionId);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<CreateAuctionState, AuctionCreatedImage, TException> context, IBehavior<CreateAuctionState, AuctionCreatedImage> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-create");
    }
}