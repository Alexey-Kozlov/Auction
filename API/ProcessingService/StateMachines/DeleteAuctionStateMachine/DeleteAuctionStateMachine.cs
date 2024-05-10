using Contracts;
using MassTransit;
using ProcessingService.StateMachines.DeleteAuctionStateMachine;

namespace ProcessingService.StateMachines.UpdateAuctionStateMachine;
public class DeleteAuctionStateMachine : MassTransitStateMachine<DeleteAuctionState>
{
    public State AuctionDeletedState { get; }
    public State AuctionDeletedBidState { get; }
    public State AuctionDeletedGatewayState { get; }
    public State AuctionDeletedImageState { get; }
    public State AuctionDeletedNotificationState { get; }
    public State AuctionDeletedSearchState { get; }
    public State CompletedState { get; }
    public State FaultedState { get; }

    public Event<RequestAuctionDelete> RequestAuctionDeletingEvent { get; }
    public Event<AuctionDeleted> AuctionDeletedEvent { get; }
    public Event<AuctionDeletedBid> AuctionDeletedBidEvent { get; }
    public Event<AuctionDeletedGateway> AuctionDeletedGatewayEvent { get; }
    public Event<AuctionDeletedImage> AuctionDeletedImageEvent { get; }
    public Event<AuctionDeletedNotification> AuctionDeletedNotificationEvent { get; }
    public Event<AuctionDeletedSearch> AuctionDeletedSearchEvent { get; }

    public DeleteAuctionStateMachine()
    {
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureAuctionDeleted();
        ConfigureAuctionDeletedBid();
        ConfigureAuctionDeletedGateway();
        ConfigureAuctionDeletedImage();
        ConfigureAuctionDeletedSearch();
        ConfigureAuctionDeletedNotification();
        ConfigureCompleted();
        ConfigureAny();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestAuctionDeletingEvent);
        Event(() => AuctionDeletedEvent);
        Event(() => AuctionDeletedBidEvent);
        Event(() => AuctionDeletedGatewayEvent);
        Event(() => AuctionDeletedImageEvent);
        Event(() => AuctionDeletedNotificationEvent);
        Event(() => AuctionDeletedSearchEvent);
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestAuctionDeletingEvent)
            .Then(context =>
            {
                context.Saga.Id = context.Message.Id;
                context.Saga.AuctionAuthor = context.Message.AuctionAuthor;
                context.Saga.CorrelationId = context.Message.CorrelationId;
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(context => new AuctionDeleting(
                context.Message.Id,
                context.Message.AuctionAuthor,
                context.Message.CorrelationId
            ))
            .TransitionTo(AuctionDeletedState)
        );
    }

    private void ConfigureAuctionDeleted()
    {
        During(AuctionDeletedState,
        When(AuctionDeletedEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(context => new AuctionDeletingBid(
                context.Message.Id,
                context.Saga.CorrelationId))
            .TransitionTo(AuctionDeletedBidState));
    }
    private void ConfigureAuctionDeletedBid()
    {
        During(AuctionDeletedBidState,
        When(AuctionDeletedBidEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(context => new AuctionDeletingGateway(
                context.Message.Id,
                context.Saga.CorrelationId))
            .TransitionTo(AuctionDeletedGatewayState));
    }
    private void ConfigureAuctionDeletedGateway()
    {
        During(AuctionDeletedGatewayState,
        When(AuctionDeletedGatewayEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(context => new AuctionDeletingImage(
                context.Message.Id,
                context.Saga.CorrelationId))
            .TransitionTo(AuctionDeletedImageState));
    }
    private void ConfigureAuctionDeletedImage()
    {
        During(AuctionDeletedImageState,
        When(AuctionDeletedImageEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(context => new AuctionDeletingSearch(
                context.Message.Id,
                context.Saga.CorrelationId))
            .TransitionTo(AuctionDeletedSearchState));
    }
    private void ConfigureAuctionDeletedSearch()
    {
        During(AuctionDeletedSearchState,
        When(AuctionDeletedSearchEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(context => new AuctionDeletingNotification(
                context.Message.Id,
                context.Saga.CorrelationId))
            .TransitionTo(AuctionDeletedNotificationState));
    }
    private void ConfigureAuctionDeletedNotification()
    {
        During(AuctionDeletedNotificationState,
        When(AuctionDeletedNotificationEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .TransitionTo(CompletedState));
    }

    private void ConfigureCompleted()
    {
        During(CompletedState);
    }


    private void ConfigureAny()
    {
        DuringAny(
            When(RequestAuctionDeletingEvent)
                .Respond(x => x.Saga)
        );
    }

}