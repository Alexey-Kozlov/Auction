using Common.Contracts;
using MassTransit;
using ProcessingService.StateMachines.DeleteAuctionStateMachine;

namespace ProcessingService.StateMachines.UpdateAuctionStateMachine;
public class DeleteAuctionStateMachine : MassTransitStateMachine<DeleteAuctionState>
{
    public State AuctionDeletedState { get; }
    public State AuctionDeletedFinanceState { get; }
    public State AuctionDeletedBidState { get; }
    public State AuctionDeletedGatewayState { get; }
    public State AuctionDeletedImageState { get; }
    public State AuctionDeletedNotificationState { get; }
    public State AuctionDeletedSearchState { get; }
    public State AuctionDeletedElkState { get; }
    public State CompletedState { get; }
    public State FaultedState { get; }

    public Event<RequestAuctionDelete> RequestAuctionDeletingEvent { get; }
    public Event<AuctionDeleted> AuctionDeletedEvent { get; }
    public Event<AuctionDeletedFinance> AuctionDeletedFinanceEvent { get; }
    public Event<AuctionDeletedBid> AuctionDeletedBidEvent { get; }
    public Event<AuctionDeletedGateway> AuctionDeletedGatewayEvent { get; }
    public Event<AuctionDeletedImage> AuctionDeletedImageEvent { get; }
    public Event<AuctionDeletedNotification> AuctionDeletedNotificationEvent { get; }
    public Event<AuctionDeletedSearch> AuctionDeletedSearchEvent { get; }
    public Event<AuctionDeletedElk> AuctionDeletedElkEvent { get; }
    public Event<GetAuctionDeleteState> AuctionDeletedStateEvent { get; }
    private IConfiguration configuration { get; }

    public DeleteAuctionStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureAuctionDeleted();
        ConfigureAuctionDeletedFinance();
        ConfigureAuctionDeletedBid();
        ConfigureAuctionDeletedGateway();
        ConfigureAuctionDeletedImage();
        ConfigureAuctionDeletedSearch();
        ConfigureAuctionDeletedNotification();
        ConfigureAuctionDeletedElk();
        ConfigureCompleted();
        ConfigureGetState();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestAuctionDeletingEvent);
        Event(() => AuctionDeletedEvent);
        Event(() => AuctionDeletedFinanceEvent);
        Event(() => AuctionDeletedBidEvent);
        Event(() => AuctionDeletedGatewayEvent);
        Event(() => AuctionDeletedImageEvent);
        Event(() => AuctionDeletedNotificationEvent);
        Event(() => AuctionDeletedSearchEvent);
        Event(() => AuctionDeletedStateEvent);
        Event(() => AuctionDeletedElkEvent);
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
            .Send(
                new Uri(configuration["QueuePaths:AuctionDeleting"]),
                context => new AuctionDeleting(
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
            .Send(
                new Uri(configuration["QueuePaths:AuctionDeletingFinance"]),
                context => new AuctionDeletingFinance(
                context.Saga.Id,
                context.Saga.AuctionAuthor,
                context.Saga.CorrelationId))
            .TransitionTo(AuctionDeletedFinanceState));
    }
    private void ConfigureAuctionDeletedFinance()
    {
        During(AuctionDeletedFinanceState,
        When(AuctionDeletedFinanceEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:AuctionDeletingBid"]),
                context => new AuctionDeletingBid(
                context.Saga.Id,
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
            .Send(
                new Uri(configuration["QueuePaths:AuctionDeletingGateway"]),
                context => new AuctionDeletingGateway(
                context.Saga.Id,
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
            .Send(
                new Uri(configuration["QueuePaths:AuctionDeletingImage"]),
                context => new AuctionDeletingImage(
                context.Saga.Id,
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
            .Send(
                new Uri(configuration["QueuePaths:AuctionDeletingSearch"]),
                context => new AuctionDeletingSearch(
                context.Saga.Id,
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
            .Send(
                new Uri(configuration["QueuePaths:AuctionDeletingNotification"]),
                context => new AuctionDeletingNotification(
                context.Saga.Id,
                context.Saga.AuctionAuthor,
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
            .Send(
                new Uri(configuration["QueuePaths:AuctionDeletingElk"]),
                context => new AuctionDeletingElk(
                context.Saga.Id,
                context.Saga.AuctionAuthor,
                context.Saga.CorrelationId))
            .TransitionTo(AuctionDeletedElkState));
    }
    private void ConfigureAuctionDeletedElk()
    {
        During(AuctionDeletedElkState,
        When(AuctionDeletedElkEvent)
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


    private void ConfigureGetState()
    {
        DuringAny(
            When(AuctionDeletedStateEvent)
                .Respond(x => x.Saga)
        );
    }

}