using Common.Contracts.Auction;
using Common.Contracts.Bid;
using Common.Contracts.EventSourcing;
using Common.Contracts.Finance;
using Common.Contracts.Image;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using MassTransit;
using ProcessingService.Activities.Restore;

namespace ProcessingService.StateMachines.RestoreStateMachine;
public class RestoreStateMachine : MassTransitStateMachine<RestoreState>
{
    public State GetRecordsState { get; }
    public State GetImagesState { get; }
    public State BidState { get; }
    public State FinanceState { get; }
    public State SearchState { get; }
    public State NotifyState { get; }
    public State CommitState { get; }
    public State CompletedState { get; }


    public Event<RequestRestoreItems> RequestEvent { get; }
    public Event<ESLog_RestoreItems> EsLogItemsEvent { get; }
    public Event<ESLog_RestoreImages> EsLogImagesEvent { get; }
    public Event<ESLog_ProcessImages> ProcessImagesEvent { get; }
    public Event<BidRestoreSnapShot> BidEvent { get; }
    public Event<FinanceRestoreSnapShot> FinanceEvent { get; }
    public Event<SearchRestoreSnapShot> SearchEvent { get; }
    public Event<NotifyRestoreSnapShot> NotifyEvent { get; }
    public Event<RestoreSnapShotESCommit> CommitEvent { get; }
    public Event<RestoreSnapShotComplete> CompleteEvent { get; }

    private IConfiguration configuration { get; }
    private DataForProcessingServicesList ListItems { get; set; }
    private Guid InstanceCorrelationId { get; set; }
    private string NotifyMessage { get; set; }
    private float AllItemsCount { get; set; }
    private float ItemsCount { get; set; }
    private object locker = new();
    private float ProgressCurrent { get; set; }
    private string SessionId { get; set; }

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
        ConfigureGetImagesState();
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
        Event(() => EsLogItemsEvent, x => x.CorrelateById(p => InstanceCorrelationId));
        Event(() => EsLogImagesEvent, x => x.CorrelateById(p => InstanceCorrelationId));
        Event(() => ProcessImagesEvent, x => x.CorrelateById(p => InstanceCorrelationId));
        Event(() => BidEvent, x => x.CorrelateById(p => InstanceCorrelationId));
        Event(() => FinanceEvent, x => x.CorrelateById(p => InstanceCorrelationId));
        Event(() => SearchEvent, x => x.CorrelateById(p => InstanceCorrelationId));
        Event(() => NotifyEvent, x => x.CorrelateById(p => InstanceCorrelationId));
        Event(() => CommitEvent, x => x.CorrelateById(p => InstanceCorrelationId));
        Event(() => CompleteEvent, x => x.CorrelateById(p => InstanceCorrelationId));
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
                InstanceCorrelationId = context.Message.CorrelationId;
                ProgressCurrent = 0; //Текущей прогресс в процентах
                SessionId = context.Message.SessionId;
                context.Saga.ResetLog = context.Message.ResetLog;
                AllItemsCount = -1;
                ItemsCount = 0;
            })
        //посылаем через Кафку - удаление всех записей в BiddingService,FinanceService,
        //NotificationService,SearchService,ImageService
        .Activity(p => p.OfType<ESLogActivityReset>())
        .TransitionTo(GetRecordsState)
        );
        OnUnhandledEvent(async e => await e.Ignore());
        SetCompletedWhenFinalized();
    }

    private void ConfigureGetRecordsState()
    {
        During(GetRecordsState,
        When(EsLogItemsEvent)
            .Send(
                //сообщение для отслеживания прогресса
                new Uri(configuration["QueuePaths:RestoreProgressNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = SessionId,
                    Props = (ProgressCurrent = 5).ToString()
                })
            // посылаем через Кафку - получение из ES лог всех записей (кроме изображений) 
            // по восстановлению БД для сервисов BiddingService,FinanceService,NotificationService,SearchService
            // для восстановления изображений для сервиса ImageServices - ниже
            .Activity(p => p.OfType<ESLogActivityGetRecords>())
            .TransitionTo(GetImagesState));
    }

    private void ConfigureGetImagesState()
    {
        During(GetImagesState,
        When(EsLogImagesEvent)
            // .Then(context =>
            // {
            //     Console.WriteLine("eslog_restoreimages count " + context.Message.AllItemsCount + " batch " +
            //     context.Message.BatchCount);
            // })
            //записей аукционов не найдено, делаем уведомление и завершаем процесс
            //сообщение для отслеживания прогресса, передаем признак "-1" что ничего не найдено
            .If(context => context.Message.BatchCount == -1 && context.Message.DataItems.DataObjects.Count() == 0,
            p => p
                .Send(
                    new Uri(configuration["QueuePaths:RestoreProgressNotificationConsumer"]),
                    context => new DataForProcessingServicesList<NotifyItem>
                    {
                        DataObjects = new List<DataForProcessingService>(),
                        CorrelationId = context.Saga.CorrelationId,
                        CallBackType = SessionId,
                        Props = "-1"
                    })
                .Send(
                    new Uri(configuration["QueuePaths:RestoreNotificationConsumer"]),
                    context => new DataForProcessingServicesList<NotifyItem>
                    {
                        DataObjects = new List<DataForProcessingService>(),
                        CorrelationId = context.Saga.CorrelationId,
                        CallBackType = "",
                        Props = "Записей не найдено"
                    })
                .Publish(new RestoreSnapShotComplete
                {
                    CorrelationId = InstanceCorrelationId
                })
                .TransitionTo(CompletedState)
            )

            // вызываем активити ESLogActivityGetImages - после получения всех записей аукционов,
            //для получения записей изображений
            //BatchCount == -1 - признак получения текстовых записей аукционов
            .If(context => context.Message.BatchCount == -1 && context.Message.DataItems.DataObjects.Count() > 0,
            p => p
                .Then(q =>
                {
                    //Console.WriteLine("ESLogActivityGetImages count " + q.Message.DataItems.DataObjects.Count());
                    if (q.Message.DataItems.DataObjects.Count() > 0)
                    {
                        lock (locker) { ProgressCurrent = 10; }
                        //пришел набор записей (кроме изображений), сохраняем набор в ListItems
                        ListItems = q.Message.DataItems;
                    }
                })
                .Activity(p => p.OfType<ESLogActivityGetImages>())
            )

        //Посылаем сообщения в ImageService для обработки, BatchCount != -1 - признак получения записей изображений
        .If(context => context.Message.BatchCount != -1,
            p => p
            .Then(context =>
            {
                lock (locker)
                {
                    //инициализируем 1 раз счетчик обработанных записей, 
                    //потом будем уменьшать при каждой обработанной записи пока не станет равен == 0
                    if (AllItemsCount == -1)
                    {
                        AllItemsCount = context.Message.AllItemsCount;
                    }
                }
            })
            .Send(
                new Uri(configuration["QueuePaths:ImageRestoreConsumer"]),
                    context => new DataForProcessingServicesList<ImageDTO>
                    {
                        DataObjects = context.Message.DataItems.DataObjects,
                        CorrelationId = context.Saga.CorrelationId,
                        CallBackType = "Common.Contracts.Processing.ESLog_ProcessImages"
                    })
        ),

        //отлавливаем сообщения окончания обработки изображений - увеличиваем каждый раз счетчик изображений
        When(ProcessImagesEvent)
        .Then(context =>
        {
            lock (locker)
            {
                ItemsCount++;
                ProgressCurrent = 10 + ItemsCount * 65 / AllItemsCount;
            }
        })
        .Send(
            new Uri(configuration["QueuePaths:RestoreProgressNotificationConsumer"]),
            context => new DataForProcessingServicesList<NotifyItem>
            {
                DataObjects = new List<DataForProcessingService>(),
                CorrelationId = context.Saga.CorrelationId,
                CallBackType = SessionId,
                Props = ProgressCurrent.ToString()
            })
        //конец обработки изображений - переходим для обработки текстовых соображений
        .If(context => AllItemsCount == ItemsCount,
            p => p
            .Publish(new BidRestoreSnapShot
            {
                CorrelationId = InstanceCorrelationId
            })
            .TransitionTo(BidState))
        );
    }
    private void ConfigureBidState()
    {
        During(BidState,
        When(BidEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
                NotifyMessage += $", Ставок - {ListItems.DataObjects.Where(p => p.DataType == "BidItem").Count()}";
            })
            .Send(
                new Uri(configuration["QueuePaths:RestoreProgressNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = SessionId,
                    Props = (ProgressCurrent = 80).ToString()
                })
            //Обновление ставок (если есть) в сервисе BiddingService
            .IfElse(context => ListItems.DataObjects.Any(p => p.DataType == "BidItem"),
                p => p
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
                NotifyMessage += $", Финансов - {ListItems.DataObjects.Where(p => p.DataType == "FinanceItem").Count()}";
            })
            .Send(
                new Uri(configuration["QueuePaths:RestoreProgressNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = SessionId,
                    Props = (ProgressCurrent = 85).ToString()
                })
            //Обновление денег (если есть) в сервисе FinanceService
            .IfElse(context => ListItems.DataObjects.Any(p => p.DataType == "FinanceItem"),
                p => p
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
                NotifyMessage += $", Аукционов - {ListItems.DataObjects.Where(p => p.DataType == "AuctionItem").Count()}";
            })
            .Send(
                new Uri(configuration["QueuePaths:RestoreProgressNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = SessionId,
                    Props = (ProgressCurrent = 90).ToString()
                })
            //Обновление записей аукционов (если есть) в сервисе SearchService
            .IfElse(context => ListItems.DataObjects.Any(p => p.DataType == "AuctionItem"),
                p => p
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
                NotifyMessage += $", Уведомлений - {ListItems.DataObjects.Where(p => p.DataType == "NotifyItem").Count()}";
            })
            .Send(
                new Uri(configuration["QueuePaths:RestoreProgressNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = SessionId,
                    Props = (ProgressCurrent = 95).ToString()
                })
            //Обновление записей уведомлений (если есть) в сервисе NotifyService
            .Send(
                new Uri(configuration["QueuePaths:RestoreNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = ListItems.DataObjects.Where(p => p.DataType == "NotifyItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.EventSourcing.RestoreSnapShotESCommit",
                    Props = NotifyMessage
                })
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
            .Send(
                new Uri(configuration["QueuePaths:RestoreProgressNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = SessionId,
                    Props = "100"
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