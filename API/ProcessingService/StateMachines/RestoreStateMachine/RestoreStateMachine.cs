using Common.Contracts.Auction;
using Common.Contracts.Bid;
using Common.Contracts.EventSourcing;
using Common.Contracts.Finance;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using MassTransit;
using ProcessingService.Activities.Restore;

namespace ProcessingService.StateMachines.RestoreStateMachine;
public class RestoreStateMachine : MassTransitStateMachine<RestoreState>
{
    public State GetRecordsState { get; }
    public State BidState { get; }
    public State FinanceState { get; }
    public State SearchState { get; }
    public State NotifyState { get; }
    public State CommitState { get; }
    public State CompletedState { get; }


    public Event<RequestRestoreSnapShot> RequestEvent { get; }
    public Event<ESLog_ResetSnapShot> GetRecordsEvent { get; }
    public Event<ESLog_RestoreSnapShot> EsLogEvent { get; }
    public Event<FinanceRestoreSnapShot> FinanceEvent { get; }
    public Event<SearchRestoreSnapShot> SearchEvent { get; }
    public Event<NotifyRestoreSnapShot> NotifyEvent { get; }
    public Event<RestoreSnapShotESCommit> CommitEvent { get; }
    public Event<RestoreSnapShotComplete> CompleteEvent { get; }

    private IConfiguration configuration { get; }
    private DataForProcessingServicesList ListItems { get; set; }
    private Guid InstanceCorrelationId { get; set; }
    private string NotifyMessage { get; set; }

    public RestoreStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureGetRecordsState();
        ConfigureBidState();
        ConfigureFinanceState();
        ConfigureSearchState();
        ConfigureNotifyState();
        ConfigureCommitState();
        ConfigureCompletedState();

    }
    private void ConfigureEvents()
    {
        Event(() => RequestEvent, p =>
        {
            p.InsertOnInitial = true;
        });
        Event(() => GetRecordsEvent);
        Event(() => EsLogEvent);
        Event(() => FinanceEvent);
        Event(() => SearchEvent);
        Event(() => NotifyEvent);
        Event(() => CommitEvent);
        Event(() => CompleteEvent);
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestEvent)
            .Then(context =>
            {
                context.Saga.CorrelationId = context.Message.CorrelationId;
                context.Saga.LastUpdated = DateTime.UtcNow;
                context.Saga.RestoreDate = context.Message.RestoreDate;
                context.Saga.UserLogin = context.Message.UserLogin;
                NotifyMessage = "Восстановление данных завершено";
            })
        //посылаем через Кафку - удаление всех записей в BiddingService,FinanceService,NotificationService,SearchService
        .Activity(p => p.OfType<ESLogActivityReset>())
        .TransitionTo(GetRecordsState)
        );
    }

    private void ConfigureGetRecordsState()
    {
        During(GetRecordsState,
        When(GetRecordsEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
                InstanceCorrelationId = context.Saga.CorrelationId;
            })
            //посылаем через Кафку - получение из ES лог всех записей по восстановлению БД для сервисов
            // BiddingService,FinanceService,NotificationService,SearchService
            .Activity(p => p.OfType<ESLogActivityGetRecords>())
            .TransitionTo(BidState));
    }

    private void ConfigureBidState()
    {
        During(BidState,
        When(EsLogEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
                ListItems = context.Message.DataItems;
            })
            //Обновление ставок (если есть) в сервисе BiddingService
            .IfElse(context => ListItems.DataObjects.Any(p => p.DataType == "BidItem"),
                p => p
                .Then(mes =>
                {
                    NotifyMessage += $", Ставок - {ListItems.DataObjects.Where(p => p.DataType == "BidItem").Count()}";
                })
                .Send(
                new Uri(configuration["QueuePaths:BidConsumer"]),
                context => new DataForProcessingServicesList<BidItem>
                {
                    DataObjects = ListItems.DataObjects.Where(p => p.DataType == "BidItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.EventSourcing.FinanceRestoreSnapShot"
                }),
                p => p
                .Publish(new FinanceRestoreSnapShot
                {
                    CorrelationId = InstanceCorrelationId
                }))
            .TransitionTo(FinanceState));
    }

    private void ConfigureFinanceState()
    {
        During(FinanceState,
        When(FinanceEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //Обновление денег (если есть) в сервисе FinanceService
            .IfElse(context => ListItems.DataObjects.Any(p => p.DataType == "FinanceItem"),
                p => p
                .Then(mes =>
                {
                    NotifyMessage += $", Финансов - {ListItems.DataObjects.Where(p => p.DataType == "FinanceItem").Count()}";
                })
                .Send(
                new Uri(configuration["QueuePaths:FinanceConsumer"]),
                context => new DataForProcessingServicesList<FinanceItem>
                {
                    DataObjects = ListItems.DataObjects.Where(p => p.DataType == "FinanceItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.EventSourcing.SearchRestoreSnapShot"
                }),
                p => p
                .Publish(new SearchRestoreSnapShot
                {
                    CorrelationId = InstanceCorrelationId
                }))
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
            //Обновление записей аукционов (если есть) в сервисе SearchService
            .IfElse(context => ListItems.DataObjects.Any(p => p.DataType == "AuctionItem"),
                p => p
                .Then(mes =>
                {
                    NotifyMessage += $", Аукционов - {ListItems.DataObjects.Where(p => p.DataType == "AuctionItem").Count()}";
                })
                .Send(
                new Uri(configuration["QueuePaths:SearchConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = ListItems.DataObjects.Where(p => p.DataType == "AuctionItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.EventSourcing.NotifyRestoreSnapShot"
                }),
                p => p
                .Publish(new NotifyRestoreSnapShot
                {
                    CorrelationId = InstanceCorrelationId
                }))
            .TransitionTo(NotifyState));
    }

    private void ConfigureNotifyState()
    {
        During(NotifyState,
        When(NotifyEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //Обновление записей аукционов (если есть) в сервисе SearchService
            .IfElse(context => ListItems.DataObjects.Any(p => p.DataType == "NotifyItem"),
                p => p
                .Then(mes =>
                {
                    NotifyMessage += $", Уведомлений - {ListItems.DataObjects.Where(p => p.DataType == "NotifyItem").Count()}";
                })
                .Send(
                new Uri(configuration["QueuePaths:RestoreNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = ListItems.DataObjects.Where(p => p.DataType == "NotifyItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.EventSourcing.RestoreSnapShotESCommit",
                    Props = NotifyMessage
                }),
                p => p
                .Publish(new RestoreSnapShotESCommit
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
            .Finalize()
        );
    }



}