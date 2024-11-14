using Common.Contracts.Auction;
using MassTransit;
using ProcessingService.Activities.AuctionCreate;

namespace ProcessingService.StateMachines.CreateAuctionStateMachine;
public class CreateAuctionStateMachine : MassTransitStateMachine<CreateAuctionState>
{
    public State BidState { get; }
    public State ImageState { get; }
    public State SearchState { get; }
    public State ElkState { get; }
    public State NotificationState { get; }
    public State CommitState { get; }
    public State CompletedState { get; }


    public Event<RequestAuctionCreate> RequestEvent { get; }
    public Event<AuctionCreatedBid> BidEvent { get; }
    public Event<AuctionCreatedImage> ImageEvent { get; }
    public Event<AuctionCreatedSearch> SearchEvent { get; }
    public Event<AuctionCreatedElk> ElkEvent { get; }
    public Event<AuctionCreatedNotification> NotificationEvent { get; }
    public Event<AuctionCreateESCommit> CommitEvent { get; }

    private string Image { get; set; }
    private IConfiguration configuration { get; }

    public CreateAuctionStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureBidState();
        ConfigureImageState();
        ConfigureSearchState();
        ConfigureNotificationState();
        ConfigureElkState();
        ConfigureCommitState();
        ConfigureCompletedState();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestEvent, p =>
        {
            p.InsertOnInitial = true;
        });
        Event(() => BidEvent);
        Event(() => ImageEvent);
        Event(() => SearchEvent);
        Event(() => ElkEvent);
        Event(() => NotificationEvent);
        Event(() => CommitEvent);
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestEvent)
            .Then(context =>
            {
                context.Saga.AuctionId = context.Message.AuctionId;
                context.Saga.Title = context.Message.Title;
                context.Saga.Description = context.Message.Description;
                context.Saga.Properties = context.Message.Properties;
                context.Saga.UserLogin = context.Message.UserLogin;
                context.Saga.AuctionEnd = context.Message.AuctionEnd;
                context.Saga.CorrelationId = context.Message.CorrelationId;
                context.Saga.LastUpdated = DateTime.UtcNow;
                context.Saga.ReservePrice = context.Message.ReservePrice;
                this.Image = context.Message.Image;
            })
            //посылаем через Кафку
            //Создание записи по обновлению аукциона в сервисе BiddingService (обновление времени окончания)
            .TransitionTo(BidState)
        );
    }

    private void ConfigureBidState()
    {
        During(BidState,
        When(BidEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //посылаем в ImageService - для создания изображения
            .Send(
                new Uri(configuration["QueuePaths:AuctionCreatingImage"]),
                context => new AuctionCreatingImage(
                context.Saga.AuctionId,
                this.Image,
                context.Saga.CorrelationId))
            .TransitionTo(ImageState));
    }

    private void ConfigureImageState()
    {
        During(ImageState,
        When(ImageEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //посылаем через Кафку в EventSourcingService -> CreateEventSourcingItemConsumer
            //Создание записи по обновлению аукциона в сервисе SearchService (обновление всех описаний)
            .Activity(p => p.OfType<SearchActivity>())
            .TransitionTo(SearchState));
    }

    private void ConfigureSearchState()
    {
        During(SearchState,
        When(SearchEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //посылаем через Кафку в EventSourcingService -> CreateEventSourcingItemConsumer
            //Создание записи по обновлению аукциона в сервисе NotificationService 
            //создание подписки автора аукциона на уведомления от этого аукциона
            .Activity(p => p.OfType<NotificationActivity>())
            .TransitionTo(NotificationState));
    }

    private void ConfigureNotificationState()
    {
        During(NotificationState,
        When(NotificationEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //посылаем в EldsticSearchService - для создания новой записи в индексе поиска
            .Send(
                new Uri(configuration["QueuePaths:AuctionCreatingElk"]),
                context => new AuctionCreatingElk()
                {
                    AuctionId = context.Saga.AuctionId,
                    Title = context.Saga.Title,
                    Properties = context.Saga.Properties,
                    Description = context.Saga.Description,
                    UserLogin = context.Saga.UserLogin,
                    AuctionEnd = context.Saga.AuctionEnd,
                    AuctionCreated = DateTime.UtcNow,
                    CorrelationId = context.Saga.CorrelationId,
                    ReservePrice = context.Saga.ReservePrice,
                    ItemSold = false,
                    Winner = "",
                    Amount = 0
                })
            .TransitionTo(ElkState));
    }

    private void ConfigureElkState()
    {
        During(ElkState,
        When(ElkEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
                context.Saga.Image = string.IsNullOrEmpty(this.Image) ? "" : "Добавлено изображение";
            })
            //посылаем через Кафку в EventSourcingService - для подтверждения транзакции
            .Activity(p => p.OfType<CommitActivity>())
            .TransitionTo(CommitState));
    }

    private void ConfigureCommitState()
    {
        During(CommitState,
        When(CommitEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .TransitionTo(CompletedState));
    }

    private void ConfigureCompletedState()
    {
        During(CompletedState);
    }

}