using Common.Contracts;
using MassTransit;
using ProcessingService.Activities;

namespace ProcessingService.StateMachines.CreateAuctionStateMachine;
public class CreateAuctionStateMachine : MassTransitStateMachine<CreateAuctionState>
{
    public State AuctionCreatedState { get; }
    public State AuctionCreatedBidState { get; }
    public State AuctionCreatedImageState { get; }
    public State AuctionCreatedNotificationState { get; }
    public State AuctionCreatedSearchState { get; }
    public State AuctionCreatedElkState { get; }
    public State CommitAuctionCreatedState { get; }
    public State CompletedState { get; }
    public State FaultedState { get; }
    private IConfiguration configuration { get; }

    public Event<RequestAuctionCreate> RequestAuctionCreatingEvent { get; }
    public Event<AuctionCreatedBid> AuctionCreatedBidEvent { get; }

    public Event<AuctionCreatedImage> AuctionCreatedImageEvent { get; }
    public Event<AuctionCreatedNotification> AuctionCreatedNotificationEvent { get; }
    public Event<AuctionCreatedSearch> AuctionCreatedSearchEvent { get; }
    public Event<AuctionCreatedElk> AuctionCreatedElkEvent { get; }
    public Event<CommitAuctionCreatedContract> CommitAuctionCreatedEvent { get; }
    public Event<GetAuctionCreateState> AuctionCreatedStateEvent { get; }
    private string Image { get; set; }

    public CreateAuctionStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureAuctionCreatedBid();
        ConfigureAuctionCreatedImage();
        ConfigureAuctionCreatedSearch();
        ConfigureAuctionCreatedNotification();
        ConfigureAuctionCreatedElk();
        ConfigureCommitCreatingAuction();
        ConfigureCompleted();
        ConfigureAny();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestAuctionCreatingEvent, p =>
        {
            p.InsertOnInitial = true;
        });
        Event(() => AuctionCreatedBidEvent);
        Event(() => AuctionCreatedImageEvent);
        Event(() => AuctionCreatedNotificationEvent);
        Event(() => AuctionCreatedSearchEvent);
        Event(() => AuctionCreatedStateEvent);
        Event(() => AuctionCreatedElkEvent);
        Event(() => CommitAuctionCreatedEvent);
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
                context.Saga.CorrelationId = context.Message.CorrelationId;
                context.Saga.LastUpdated = DateTime.UtcNow;
                context.Saga.ReservePrice = context.Message.ReservePrice;
                this.Image = context.Message.Image;
            })
            //посылаем в BiddingService - для создания новой записи аукциона
            .Send(
                new Uri(configuration["QueuePaths:AuctionCreatingBid"]),
                context => new AuctionCreatingBid(
                context.Saga.Id,
                context.Saga.AuctionEnd,
                context.Saga.AuctionAuthor,
                context.Saga.CorrelationId,
                context.Saga.ReservePrice))
            .TransitionTo(AuctionCreatedBidState)
        );
    }

    private void ConfigureAuctionCreatedBid()
    {
        During(AuctionCreatedBidState,
        When(AuctionCreatedBidEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //посылаем в ImageService - для создания изображения
            .Send(
                new Uri(configuration["QueuePaths:AuctionCreatingImage"]),
                context => new AuctionCreatingImage(
                context.Saga.Id,
                this.Image,
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
            //посылаем в SearchService - для создания новой текстовой записи для просмотра
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
            //посылаем в NotificationService - для уведомления о новом аукционе
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
            //посылаем в EldsticSearchService - для создания новой записи в индексе поиска
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
                context.Saga.Image = string.IsNullOrEmpty(this.Image) ? "" : "Добавлено изображение";
            })
            //посылаем через Кафку в EventSourcingService - для подтверждения транзакции
            .Activity(p => p.OfType<CommitCreatingAuctionActivity>())
            .TransitionTo(CommitAuctionCreatedState));
    }

    private void ConfigureCommitCreatingAuction()
    {
        During(CommitAuctionCreatedState,
        When(CommitAuctionCreatedEvent)
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
            When(AuctionCreatedStateEvent)
                .Respond(x => x.Saga)
        );
    }

}