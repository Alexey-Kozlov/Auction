using Common.Contracts;
using MassTransit;

namespace ProcessingService.StateMachines.CreateAuctionStateMachine;
public class CreateAuctionStateMachine : MassTransitStateMachine<CreateAuctionState>
{
    public State AuctionCreatedState { get; }
    public State AuctionCreatedBidState { get; }
    public State AuctionCreatedImageState { get; }
    public State AuctionCreatedNotificationState { get; }
    public State AuctionCreatedSearchState { get; }
    public State AuctionCreatedElkState { get; }
    public State CompletedState { get; }
    public State FaultedState { get; }
    private IConfiguration configuration { get; }

    public Event<RequestAuctionCreate> RequestAuctionCreatingEvent { get; }
    public Event<AuctionCreated> AuctionCreatedEvent { get; }
    public Event<AuctionCreatedBid> AuctionCreatedBidEvent { get; }
    public Event<AuctionCreatedImage> AuctionCreatedImageEvent { get; }
    public Event<AuctionCreatedNotification> AuctionCreatedNotificationEvent { get; }
    public Event<AuctionCreatedSearch> AuctionCreatedSearchEvent { get; }
    public Event<AuctionCreatedElk> AuctionCreatedElkEvent { get; }
    public Event<GetAuctionCreateState> AuctionCreatedStateEvent { get; }

    public CreateAuctionStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureAuctionCreated();
        ConfigureAuctionCreatedBid();
        ConfigureAuctionCreatedImage();
        ConfigureAuctionCreatedSearch();
        ConfigureAuctionCreatedNotification();
        ConfigureAuctionCreatedElk();
        ConfigureCompleted();
        ConfigureAny();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestAuctionCreatingEvent);
        Event(() => AuctionCreatedEvent);
        Event(() => AuctionCreatedBidEvent);
        Event(() => AuctionCreatedImageEvent);
        Event(() => AuctionCreatedNotificationEvent);
        Event(() => AuctionCreatedSearchEvent);
        Event(() => AuctionCreatedStateEvent);
        Event(() => AuctionCreatedElkEvent);
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestAuctionCreatingEvent)
            .Then(context =>
            {
                context.Saga.Id = context.Message.Id;
                context.Saga.Title = context.Message.Title;
                context.Saga.Description = context.Message.Description;
                context.Saga.Properties = context.Message.Properties;
                context.Saga.AuctionAuthor = context.Message.AuctionAuthor;
                context.Saga.AuctionEnd = context.Message.AuctionEnd;
                context.Saga.Image = context.Message.Image;
                context.Saga.CorrelationId = context.Message.CorrelationId;
                context.Saga.LastUpdated = DateTime.UtcNow;
                context.Saga.ReservePrice = context.Message.ReservePrice;
            })
            .Send(
                new Uri(configuration["QueuePaths:AuctionCreating"]),
                context => new AuctionCreating(
                context.Message.Id,
                context.Message.Title,
                context.Message.Properties,
                context.Message.Image,
                context.Message.Description,
                context.Message.AuctionAuthor,
                context.Message.AuctionEnd,
                context.Message.CorrelationId,
                context.Message.ReservePrice
            ))
            .TransitionTo(AuctionCreatedState)
        );
    }

    private void ConfigureAuctionCreated()
    {
        During(AuctionCreatedState,
        When(AuctionCreatedEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:AuctionCreatingBid"]),
                context => new AuctionCreatingBid(
                context.Saga.Id,
                context.Saga.AuctionEnd,
                context.Saga.AuctionAuthor,
                context.Saga.CorrelationId,
                context.Saga.ReservePrice))
            .TransitionTo(AuctionCreatedBidState));
    }
    private void ConfigureAuctionCreatedBid()
    {
        During(AuctionCreatedBidState,
        When(AuctionCreatedBidEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:AuctionCreatingImage"]),
                context => new AuctionCreatingImage(
                context.Saga.Id,
                context.Saga.Image,
                context.Saga.CorrelationId))
            .TransitionTo(AuctionCreatedImageState));
    }

    private void ConfigureAuctionCreatedImage()
    {
        During(AuctionCreatedImageState,
        When(AuctionCreatedImageEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:AuctionCreatingSearch"]),
                context => new AuctionCreatingSearch(
                context.Saga.Id,
                context.Saga.Title,
                context.Saga.Properties,
                context.Saga.Description,
                context.Saga.AuctionAuthor,
                context.Saga.AuctionEnd,
                context.Saga.CorrelationId,
                context.Saga.ReservePrice))
            .TransitionTo(AuctionCreatedSearchState));
    }
    private void ConfigureAuctionCreatedSearch()
    {
        During(AuctionCreatedSearchState,
        When(AuctionCreatedSearchEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:AuctionCreatingNotification"]),
                context => new AuctionCreatingNotification(
                context.Saga.Id,
                context.Saga.AuctionAuthor,
                context.Saga.Title,
                context.Saga.CorrelationId))
            .TransitionTo(AuctionCreatedNotificationState));
    }

    private void ConfigureAuctionCreatedNotification()
    {
        During(AuctionCreatedNotificationState,
        When(AuctionCreatedNotificationEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:AuctionCreatingElk"]),
                context => new AuctionCreatingElk()
                {
                    Id = context.Saga.Id,
                    Title = context.Saga.Title,
                    Properties = context.Saga.Properties,
                    Description = context.Saga.Description,
                    AuctionAuthor = context.Saga.AuctionAuthor,
                    AuctionEnd = context.Saga.AuctionEnd,
                    AuctionCreated = DateTime.UtcNow,
                    CorrelationId = context.Saga.CorrelationId,
                    ReservePrice = context.Saga.ReservePrice,
                    ItemSold = false,
                    Winner = "",
                    Amount = 0
                })
            .TransitionTo(AuctionCreatedElkState));
    }

    private void ConfigureAuctionCreatedElk()
    {
        During(AuctionCreatedElkState,
        When(AuctionCreatedElkEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
                //очищаем поле изображения (если оно было), чтобы не засорять БД
                context.Saga.Image = string.IsNullOrEmpty(context.Saga.Image) ? "" : "Добавлено изображение";
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
            When(AuctionCreatedStateEvent)
                .Respond(x => x.Saga)
        );
    }

}