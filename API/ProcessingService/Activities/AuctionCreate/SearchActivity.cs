using Common.Contracts;
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
            new AuctionCreatingSearch(
                context.Saga.AuctionId,
                context.Saga.Title,
                context.Saga.Properties,
                context.Saga.Description,
                context.Saga.UserLogin,
                context.Saga.AuctionEnd,
                context.Saga.CorrelationId,
                context.Saga.ReservePrice
            ),
            nameof(AuctionCreatingSearch),
            _config["ServicesName:SearchService"],
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            OperationType.Insert,
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