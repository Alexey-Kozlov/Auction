using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Bid;
using Common.Contracts.Finance;
using Common.Contracts.Notification;
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
    public State EndState { get; }

    public Event<RequestAuctionDelete> RequestEvent { get; }
    public Event<ESLog_AuctionDeleted> EsLogEvent { get; }
    public Event<AuctionDeletedBid> BidEvent { get; }
    public Event<AuctionDeletedGateway> GatewayEvent { get; }
    public Event<AuctionDeletedImage> ImageEvent { get; }
    public Event<AuctionDeletedSearch> SearchEvent { get; }
    public Event<AuctionDeletedElk> ElkEvent { get; }
    public Event<AuctionDeletedNotification> NotificationEvent { get; }
    public Event<AuctionDeleteESCommit> CommitEvent { get; }
    public Event<AuctionDeleteComplete> CompleteEvent { get; }
    private IConfiguration configuration { get; }
    private DataForProcessingServicesList ListItems { get; set; }
    private Guid InstanceCorrelationId { get; set; }

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
        ConfigureCompletedState();
        ConfigureEndState();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestEvent, p =>
        {
            p.InsertOnInitial = true;
        });
        Event(() => EsLogEvent);
        Event(() => BidEvent, x => x.CorrelateById(p => InstanceCorrelationId));
        Event(() => GatewayEvent, x => x.CorrelateById(p => InstanceCorrelationId));
        Event(() => ImageEvent);
        Event(() => SearchEvent);
        Event(() => ElkEvent);
        Event(() => NotificationEvent);
        Event(() => CommitEvent, x => x.CorrelateById(p => InstanceCorrelationId));
        Event(() => CompleteEvent);
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
                InstanceCorrelationId = context.Message.CorrelationId;
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
            //Удаление записи (деньги на ставку, если есть) в сервисе FinanceService
            .IfElse(context => ListItems.DataObjects.Any(p => p.DataType == "FinanceItem"),
                p => p
                .Send(
                new Uri(configuration["QueuePaths:FinanceConsumer"]),
                context => new DataForProcessingServicesList<FinanceItem>
                {
                    DataObjects = ListItems.DataObjects.Where(p => p.DataType == "FinanceItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionDeletedBid"
                }),
                p => p
                .Publish(new AuctionDeletedBid
                {
                    CorrelationId = InstanceCorrelationId
                }))
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
            //Удаление записей (ставка, если есть) в сервисе BiddingService
            .IfElse(context => ListItems.DataObjects.Any(p => p.DataType == "BidItem"),
                p => p
                .Send(
                new Uri(configuration["QueuePaths:BidConsumer"]),
                context => new DataForProcessingServicesList<BidItem>
                {
                    DataObjects = ListItems.DataObjects.Where(p => p.DataType == "BidItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionDeletedGateway"
                }),
                p => p
                .Publish(new AuctionDeletedGateway
                {
                    CorrelationId = InstanceCorrelationId
                }))
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
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = ListItems.DataObjects.Where(p => p.DataType == "AuctionItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionDeletedImage"
                })
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
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = ListItems.DataObjects.Where(p => p.DataType == "AuctionItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionDeletedSearch"
                })
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
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = ListItems.DataObjects.Where(p => p.DataType == "AuctionItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionDeletedElk"
                })
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
            //Удаление аукциона из поиска в сервисе ElasticSearchService
            .Send(
                new Uri(configuration["QueuePaths:ElkConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = ListItems.DataObjects.Where(p => p.DataType == "AuctionItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionDeletedNotification"
                })
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
            //Удаление уведомлений (если есть) в сервисе NotificationService
            .IfElse(context => ListItems.DataObjects.Any(p => p.DataType == "NotifyItem"),
                p => p
                .Send(
                new Uri(configuration["QueuePaths:NotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = ListItems.DataObjects.Where(p => p.DataType == "NotifyItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionDeleteESCommit",
                    Props = JsonSerializer.Deserialize<AuctionItem>(
                            ListItems.DataObjects.FirstOrDefault(p => p.DataType == "AuctionItem").Data).Title
                }),
                p => p
                .Publish(new AuctionDeleteESCommit
                {
                    CorrelationId = InstanceCorrelationId
                }))
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
            //посылаем через Кафку в EventSourcingService - для подтверждения транзакции
            .Activity(p => p.OfType<CommitActivity>())
            .TransitionTo(CompletedState));
    }

    private void ConfigureCompletedState()
    {
        During(CompletedState,
        When(CompleteEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .TransitionTo(EndState)
        );
    }

    private void ConfigureEndState()
    {
        During(EndState);
    }

}