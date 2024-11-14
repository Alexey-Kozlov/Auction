using Common.Contracts.Auction;
using Common.Contracts.Bid;
using Common.Contracts.Finance;
using Common.Contracts.Processing;
using MassTransit;
using ProcessingService.Activities.AuctionDelete;
using ProcessingService.StateMachines.DeleteAuctionStateMachine;

namespace ProcessingService.StateMachines.UpdateAuctionStateMachine;
public class DeleteAuctionStateMachine : MassTransitStateMachine<DeleteAuctionState>
{
    public State FinanceState { get; }
    public State BidState { get; }
    public State GatewayState { get; }
    public State ImageState { get; }
    public State SearchState { get; }
    public State ElkState { get; }
    public State NotificationState { get; }
    public State CommitState { get; }
    public State CompletedState { get; }

    public Event<RequestAuctionDelete> RequestEvent { get; }
    public Event<ESLog_AuctionDeleted> EsLogEvent { get; }
    public Event<AuctionDeletedBid> BidEvent { get; }
    public Event<AuctionDeletedGateway> GatewayEvent { get; }
    public Event<AuctionDeletedImage> ImageEvent { get; }
    public Event<AuctionDeletedSearch> SearchEvent { get; }
    public Event<AuctionDeletedElk> ElkEvent { get; }
    public Event<AuctionDeletedNotification> NotificationEvent { get; }
    public Event<AuctionDeleteESCommit> CommitEvent { get; }
    private IConfiguration configuration { get; }
    private DataForProcessingServicesList ListItems { get; set; }

    public DeleteAuctionStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureFinanceState();
        ConfigureBidState();
        ConfigureGatewayState();
        ConfigureImageState();
        ConfigureSearchState();
        ConfigureELKState();
        ConfigureNotificationState();
        ConfigureCommitState();
        ConfigureCompleted();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestEvent, p =>
        {
            p.InsertOnInitial = true;
        });
        Event(() => EsLogEvent);
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
        Initially(
            When(RequestEvent)
            .Then(context =>
            {
                context.Saga.AuctionId = context.Message.AuctionId;
                context.Saga.UserLogin = context.Message.UserLogin;
                context.Saga.CorrelationId = context.Message.CorrelationId;
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //посылаем через Кафку, выполнение всех операций в ES лог для удаления аукциона:
            // - Удаление записей по деньгам в сервисе FinanceService
            // - Удаление всех ставок в сервисе BiddingService
            // - Удаление записи в сервисе SearchService
            // - Удаление всех записей в сервисе NotificationService
            .Activity(p => p.OfType<ESLogActivity>())
            .TransitionTo(FinanceState)
        );
    }

    private void ConfigureFinanceState()
    {
        During(FinanceState,
        When(EsLogEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
                ListItems = context.Message.DataItems;
            })
            //Удаление записи (деньги на ставку) в сервисе FinanceService
            .If(context => ListItems.DataObjects.Any(p => p.DataType == "FinanceItem"),
                p => p
                .Send(
                new Uri(configuration["QueuePaths:FinanceConsumer"]),
                context => new DataForProcessingServicesList<FinanceItem>(
                ListItems.DataObjects.Where(p => p.DataType == "FinanceItem").ToList(),
                context.Saga.CorrelationId,
                "Common.Contracts.Auction.AuctionDeletedBid")))
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
            //Удаление записи (ставка) в сервисе BiddingService
            .If(context => ListItems.DataObjects.Any(p => p.DataType == "BidItem"),
                p => p
                .Send(
                new Uri(configuration["QueuePaths:BidConsumer"]),
                context => new DataForProcessingServicesList<BidItem>(
                ListItems.DataObjects.Where(p => p.DataType == "BidItem").ToList(),
                context.Saga.CorrelationId,
                "Common.Contracts.Auction.AuctionDeletedGateway")))
            .TransitionTo(GatewayState)
        );
    }

    private void ConfigureGatewayState()
    {
        During(GatewayState,
        When(GatewayEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //Удаление аукционв из кеша в сервисе GatewayService
            .Send(
                new Uri(configuration["QueuePaths:GatewayConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>(
                ListItems.DataObjects.Where(p => p.DataType == "AuctionItem").ToList(),
                context.Saga.CorrelationId,
                "Common.Contracts.Auction.AuctionDeletedGateway"))
            .TransitionTo(ImageState)
        );
    }

    private void ConfigureImageState()
    {
        During(ImageState,
        When(ImageEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //Удаление изображения аукциона в сервисе ImageService
            .Send(
                new Uri(configuration["QueuePaths:ImageConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>(
                ListItems.DataObjects.Where(p => p.DataType == "AuctionItem").ToList(),
                context.Saga.CorrelationId,
                "Common.Contracts.Auction.AuctionDeletedSearch"))
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
            //Удаление аукциона в сервисе SearchService
            .Send(
                new Uri(configuration["QueuePaths:SearchConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>(
                ListItems.DataObjects.Where(p => p.DataType == "AuctionItem").ToList(),
                context.Saga.CorrelationId,
                "Common.Contracts.Auction.AuctionDeletedElk"))
            .TransitionTo(ElkState));
    }

    private void ConfigureELKState()
    {
        During(ElkState,
        When(ElkEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:AuctionDeletingElk"]),
                context => new AuctionDeletingElk(
                context.Saga.AuctionId,
                context.Saga.UserLogin,
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

    private void ConfigureCompleted()
    {
        During(CompletedState);
    }

}