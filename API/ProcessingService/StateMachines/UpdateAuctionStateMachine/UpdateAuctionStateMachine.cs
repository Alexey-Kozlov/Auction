using Common.Contracts;
using MassTransit;
using ProcessingService.Activities.AuctionUpdate;

namespace ProcessingService.StateMachines.UpdateAuctionStateMachine;
public class UpdateAuctionStateMachine : MassTransitStateMachine<UpdateAuctionState>
{
    public State AuctionUpdatedState { get; }
    public State AuctionUpdatedBidState { get; }
    public State AuctionUpdatedGatewayState { get; }
    public State AuctionUpdatedImageState { get; }
    public State AuctionUpdatedNotificationState { get; }
    public State AuctionUpdatedSearchState { get; }
    public State AuctionUpdatedElkState { get; }
    public State CommitAuctionUpdatedState { get; }
    public State CompletedState { get; }
    public State FaultedState { get; }

    public Event<RequestAuctionUpdate> RequestAuctionUpdatingEvent { get; }
    public Event<AuctionUpdated> AuctionUpdatedEvent { get; }
    public Event<AuctionUpdatedBid> AuctionUpdatedBidEvent { get; }
    public Event<AuctionUpdatedGateway> AuctionUpdatedGatewayEvent { get; }
    public Event<AuctionUpdatedImage> AuctionUpdatedImageEvent { get; }
    public Event<AuctionUpdatedNotification> AuctionUpdatedNotificationEvent { get; }
    public Event<AuctionUpdatedSearch> AuctionUpdatedSearchEvent { get; }
    public Event<AuctionUpdatedElk> AuctionUpdatedElkEvent { get; }
    public Event<CommitAuctionUpdatedContract> CommitAuctionUpdatedEvent { get; }
    public Event<GetAuctionUpdateState> AuctionUpdatedStateEvent { get; }
    private IConfiguration configuration { get; }
    private string Image { get; set; }

    public UpdateAuctionStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureAuctionUpdated();
        ConfigureAuctionUpdatedBid();
        ConfigureAuctionUpdatedGateway();
        ConfigureAuctionUpdatedImage();
        ConfigureAuctionUpdatedSearch();
        ConfigureAuctionUpdatedNotification();
        ConfigureAuctionUpdatedElk();
        ConfigureCommitUpdatingAuction();
        ConfigureCompleted();
        ConfigureAny();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestAuctionUpdatingEvent);
        Event(() => AuctionUpdatedEvent);
        Event(() => AuctionUpdatedBidEvent);
        Event(() => AuctionUpdatedGatewayEvent);
        Event(() => AuctionUpdatedImageEvent);
        Event(() => AuctionUpdatedNotificationEvent);
        Event(() => AuctionUpdatedSearchEvent);
        Event(() => AuctionUpdatedElkEvent);
        Event(() => AuctionUpdatedStateEvent);
        Event(() => CommitAuctionUpdatedEvent);
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestAuctionUpdatingEvent)
            .Then(context =>
            {
                context.Saga.Id = context.Message.Id;
                context.Saga.Title = context.Message.Title;
                context.Saga.Description = context.Message.Description;
                context.Saga.Properties = context.Message.Properties;
                context.Saga.AuctionAuthor = context.Message.AuctionAuthor;
                context.Saga.AuctionEnd = context.Message.AuctionEnd;
                this.Image = context.Message.Image;
                context.Saga.CorrelationId = context.Message.CorrelationId;
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Activity(p => p.OfType<UpdatingAuctionActivity>())
            .TransitionTo(AuctionUpdatedState)
        );
    }

    private void ConfigureAuctionUpdated()
    {
        During(AuctionUpdatedState,
        When(AuctionUpdatedEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:AuctionUpdatingBid"]),
                context => new AuctionUpdatingBid(
                context.Saga.Id,
                context.Saga.AuctionEnd,
                context.Saga.CorrelationId))
            .TransitionTo(AuctionUpdatedBidState));
    }
    private void ConfigureAuctionUpdatedBid()
    {
        During(AuctionUpdatedBidState,
        When(AuctionUpdatedBidEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:AuctionUpdatingGateway"]),
                context => new AuctionUpdatingGateway(
                context.Saga.Id,
                context.Saga.CorrelationId))
            .TransitionTo(AuctionUpdatedGatewayState));
    }
    private void ConfigureAuctionUpdatedGateway()
    {
        During(AuctionUpdatedGatewayState,
        When(AuctionUpdatedGatewayEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:AuctionUpdatingImage"]),
                context => new AuctionUpdatingImage(
                context.Saga.Id,
                this.Image,
                context.Saga.CorrelationId))
            .TransitionTo(AuctionUpdatedImageState));
    }
    private void ConfigureAuctionUpdatedImage()
    {
        During(AuctionUpdatedImageState,
        When(AuctionUpdatedImageEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:AuctionUpdatingSearch"]),
                context => new AuctionUpdatingSearch(
                context.Saga.Id,
                context.Saga.Title,
                context.Saga.Properties,
                context.Saga.Description,
                context.Saga.AuctionAuthor,
                context.Saga.AuctionEnd,
                context.Saga.CorrelationId))
            .TransitionTo(AuctionUpdatedSearchState));
    }
    private void ConfigureAuctionUpdatedSearch()
    {
        During(AuctionUpdatedSearchState,
        When(AuctionUpdatedSearchEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:AuctionUpdatingNotification"]),
                context => new AuctionUpdatingNotification(
                context.Saga.Id,
                context.Saga.AuctionAuthor,
                context.Saga.CorrelationId))
            .TransitionTo(AuctionUpdatedNotificationState));
    }
    private void ConfigureAuctionUpdatedNotification()
    {
        During(AuctionUpdatedNotificationState,
        When(AuctionUpdatedNotificationEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:AuctionUpdatingElk"]),
                context => new AuctionUpdatingElk(
                context.Saga.Id,
                context.Saga.Title,
                context.Saga.Properties,
                context.Saga.Description,
                context.Saga.AuctionAuthor,
                context.Saga.AuctionEnd,
                context.Saga.CorrelationId))
            .TransitionTo(AuctionUpdatedElkState));
    }
    private void ConfigureAuctionUpdatedElk()
    {
        During(AuctionUpdatedElkState,
        When(AuctionUpdatedElkEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
                context.Saga.Image = string.IsNullOrEmpty(this.Image) ? "" : "Обновление изображения";
            })
            .Activity(p => p.OfType<CommitUpdatingAuctionActivity>())
            .TransitionTo(CommitAuctionUpdatedState));
    }

    private void ConfigureCommitUpdatingAuction()
    {
        During(CommitAuctionUpdatedState,
        When(CommitAuctionUpdatedEvent)
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
            When(AuctionUpdatedStateEvent)
                .Respond(x => x.Saga)
        );
    }

}