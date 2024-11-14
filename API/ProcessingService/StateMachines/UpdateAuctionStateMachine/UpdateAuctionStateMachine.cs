using Common.Contracts.Auction;
using MassTransit;
using ProcessingService.Activities.AuctionUpdate;

namespace ProcessingService.StateMachines.UpdateAuctionStateMachine;
public class UpdateAuctionStateMachine : MassTransitStateMachine<UpdateAuctionState>
{
    public State BidState { get; }
    public State GatewayState { get; }
    public State ImageState { get; }
    public State SearchState { get; }
    public State ElkState { get; }
    public State NotificationState { get; }
    public State ESCommitState { get; }
    public State CompletedState { get; }

    public Event<RequestAuctionUpdate> RequestEvent { get; }
    public Event<AuctionUpdatedBid> BidEvent { get; }
    public Event<AuctionUpdatedGateway> GatewayEvent { get; }
    public Event<AuctionUpdatedImage> ImageEvent { get; }
    public Event<AuctionUpdatedSearch> SearchEvent { get; }
    public Event<AuctionUpdatedElk> ElkEvent { get; }
    public Event<AuctionUpdatedNotification> NotificationEvent { get; }
    public Event<AuctionUpdateESCommit> CommitEvent { get; }
    private IConfiguration configuration { get; }
    private string Image { get; set; }

    public UpdateAuctionStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureBidState();
        ConfigureGatewayState();
        ConfigureImageState();
        ConfigureSearchState();
        ConfigureNotificationState();
        ConfigureElkState();
        ConfigureESCommitState();
        ConfigureCompletedState();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestEvent, p =>
        {
            p.InsertOnInitial = true;
        });
        Event(() => BidEvent);
        Event(() => GatewayEvent);
        Event(() => ImageEvent);
        Event(() => SearchEvent);
        Event(() => ElkEvent);
        Event(() => NotificationEvent);
        Event(() => CommitEvent);
    }
    private void ConfigureInitialState()
    {
        //инициализация
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
                this.Image = context.Message.Image;
                context.Saga.CorrelationId = context.Message.CorrelationId;
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //посылаем через Кафку в EventSourcingService -> CreateEventSourcingItemConsumer
            //Создание записи по обновлению аукциона в сервисе BiddingService (обновление времени окончания)
            //.Activity(p => p.OfType<BidActivity>())
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
            //посылаем в GatewayService - для очистки кеша по данному аукциону
            .Send(
                new Uri(configuration["QueuePaths:AuctionUpdatingGateway"]),
                context => new AuctionUpdatingGateway(
                context.Saga.AuctionId,
                context.Saga.CorrelationId))
            .TransitionTo(GatewayState));
    }
    private void ConfigureGatewayState()
    {
        During(GatewayState,
        When(GatewayEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //посылаем в ImageService - для обновления изображения аукциона
            .Send(
                new Uri(configuration["QueuePaths:AuctionUpdatingImage"]),
                context => new AuctionUpdatingImage(
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
            //посылаем в ElasticSearchService - для обновления информации в индексах поиска
            .Send(
                new Uri(configuration["QueuePaths:AuctionUpdatingElk"]),
                context => new AuctionUpdatingElk(
                context.Saga.AuctionId,
                context.Saga.Title,
                context.Saga.Properties,
                context.Saga.Description,
                context.Saga.UserLogin,
                context.Saga.AuctionEnd,
                context.Saga.CorrelationId))
            .TransitionTo(ElkState));
    }
    private void ConfigureElkState()
    {
        During(ElkState,
        When(ElkEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
                context.Saga.Image = string.IsNullOrEmpty(this.Image) ? "" : "Обновление изображения";
            })
            //посылаем через Кафку в EventSourcingService - для подтверждения транзакции
            .Activity(p => p.OfType<CommitActivity>())
            .TransitionTo(ESCommitState));
    }

    private void ConfigureESCommitState()
    {
        During(ESCommitState,
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