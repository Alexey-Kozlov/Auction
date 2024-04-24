using Contracts;
using MassTransit;

namespace ProcessingService.StateMachines;
public class ProcessiungStateMachine : MassTransitStateMachine<ProcessingState>
{
    public State AcceptedState { get; }
    public State FinanceGrantedState { get; }
    public State CompletedState { get; }
    public State FaultedState { get; }

    public Event<RequestProcessingBidStart> RequestProcessingBidStartEvent { get; }
    public Event<FinanceGranted> FinanceGrantedEvent { get; }
    public Event<BidPlaced> BidPlacedEvent { get; }
    public Event<GetProcessingBidState> GetProcessingBidStateEvent { get; }

    public ProcessiungStateMachine()
    {
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureAccepted();
        ConfigureFinanceGranted();
        ConfigureCompleted();
        ConfigureAny();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestProcessingBidStartEvent);
        Event(() => FinanceGrantedEvent);
        Event(() => BidPlacedEvent);
        Event(() => GetProcessingBidStateEvent);
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestProcessingBidStartEvent)
            .Then(context =>
            {
                context.Saga.Bidder = context.Message.Bidder;
                context.Saga.LastUpdated = DateTime.UtcNow;
                context.Saga.AuctionId = context.Message.AuctionId;
                context.Saga.Amount = context.Message.Amount;
                context.Saga.CorrelationId = context.Message.CorrelationId;
            })
            .Send(context => new RequestFinanceDebitAdd(
                context.Message.Amount,
                context.Message.AuctionId,
                context.Message.Bidder,
                context.Message.CorrelationId
            ))
            .TransitionTo(AcceptedState)
        );
    }

    private void ConfigureAccepted()
    {
        During(AcceptedState,
        When(FinanceGrantedEvent)
        .Then(context =>
        {
            context.Saga.LastUpdated = DateTime.UtcNow;
        })
        .Send(context => new RequestBidPlace(
            context.Saga.AuctionId,
            context.Saga.Bidder,
            context.Saga.Amount,
            context.Saga.CorrelationId
        ))
        .TransitionTo(FinanceGrantedState)
        );
    }

    private void ConfigureFinanceGranted()
    {
        During(FinanceGrantedState,
        When(BidPlacedEvent)
        .Then(context =>
        {
            context.Saga.LastUpdated = DateTime.UtcNow;
        })
        .TransitionTo(CompletedState)
        );
    }

    private void ConfigureCompleted()
    {
        During(CompletedState);
    }


    private void ConfigureAny()
    {
        DuringAny(
            When(GetProcessingBidStateEvent)
                .Respond(x => x.Saga)
        );
    }
}