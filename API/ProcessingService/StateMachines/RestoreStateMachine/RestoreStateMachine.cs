using System.Text.Json;
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
    public object locker = new();

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
        Event(() => RequestEvent, p => p.InsertOnInitial = true);
        Event(() => EsLogItemsEvent);
        Event(() => EsLogImagesEvent);
        Event(() => ProcessImagesEvent);
        Event(() => BidEvent);
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
                context.Saga.RestoreDate = context.Message.RestoreDate;
                context.Saga.UserLogin = context.Message.UserLogin;
                context.Saga.NotifyMessage = "Восстановление данных завершено";
                context.Saga.ProgressCurrent = 0; //Текущей прогресс в процентах
                context.Saga.SessionId = context.Message.SessionId;
                context.Saga.ResetLog = context.Message.ResetLog;
                context.Saga.AllItemsCount = -1;
                context.Saga.ItemsCount = 0;
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
                    CallBackType = context.Saga.SessionId,
                    Props = (context.Saga.ProgressCurrent = 5).ToString() + ";false"
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
                        CallBackType = context.Saga.SessionId,
                        Props = "-1" + ";true"
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
                .Publish(context => new RestoreSnapShotComplete
                {
                    CorrelationId = context.Message.CorrelationId
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
                        lock (locker) { q.Saga.ProgressCurrent = 10; }
                        //пришел набор записей (кроме изображений), сохраняем набор в ListItems
                        q.Saga.DataForProcessingServicesList = JsonSerializer.Serialize(q.Message.DataItems);
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
                    if (context.Saga.AllItemsCount == -1)
                    {
                        context.Saga.AllItemsCount = context.Message.AllItemsCount;
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
                context.Saga.ItemsCount++;
                context.Saga.ProgressCurrent = 10 + context.Saga.ItemsCount * 65 / context.Saga.AllItemsCount;
            }
        })
        .Send(
            new Uri(configuration["QueuePaths:RestoreProgressNotificationConsumer"]),
            context => new DataForProcessingServicesList<NotifyItem>
            {
                DataObjects = new List<DataForProcessingService>(),
                CorrelationId = context.Saga.CorrelationId,
                CallBackType = context.Saga.SessionId,
                Props = context.Saga.ProgressCurrent.ToString() + ";false"
            })
        //конец обработки изображений - переходим для обработки текстовых соображений
        .If(context => context.Saga.AllItemsCount == context.Saga.ItemsCount,
            p => p
            .Publish(context => new BidRestoreSnapShot
            {
                CorrelationId = context.Message.CorrelationId
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
                context.Saga.NotifyMessage += $", Ставок - {JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "BidItem").Count()}";
            })
            .Send(
                new Uri(configuration["QueuePaths:RestoreProgressNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = context.Saga.SessionId,
                    Props = (context.Saga.ProgressCurrent = 80).ToString() + ";false"
                })
            //Обновление ставок (если есть) в сервисе BiddingService
            .IfElse(context => JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Any(p => p.DataType == "BidItem"),
                p => p
                .Send(
                new Uri(configuration["QueuePaths:BidConsumer"]),
                context => new DataForProcessingServicesList<BidItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "BidItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.EventSourcing.FinanceRestoreSnapShot"
                }),
                p => p
                .Publish(context => new FinanceRestoreSnapShot
                {
                    CorrelationId = context.Message.CorrelationId
                }))
            .TransitionTo(FinanceState));
    }

    private void ConfigureFinanceState()
    {
        During(FinanceState,
        When(FinanceEvent)
            .Then(context =>
            {
                context.Saga.NotifyMessage += $", Финансов - {JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "FinanceItem").Count()}";
            })
            .Send(
                new Uri(configuration["QueuePaths:RestoreProgressNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = context.Saga.SessionId,
                    Props = (context.Saga.ProgressCurrent = 85).ToString() + ";false"
                })
            //Обновление денег (если есть) в сервисе FinanceService
            .IfElse(context => JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Any(p => p.DataType == "FinanceItem"),
                p => p
                .Send(
                new Uri(configuration["QueuePaths:FinanceConsumer"]),
                context => new DataForProcessingServicesList<FinanceItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "FinanceItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.EventSourcing.SearchRestoreSnapShot"
                }),
                p => p
                .Publish(context => new SearchRestoreSnapShot
                {
                    CorrelationId = context.Message.CorrelationId
                }))
            .TransitionTo(SearchState));
    }

    private void ConfigureSearchState()
    {
        During(SearchState,
        When(SearchEvent)
            .Then(context =>
            {
                context.Saga.NotifyMessage += $", Аукционов - {JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "AuctionItem").Count()}";
            })
            .Send(
                new Uri(configuration["QueuePaths:RestoreProgressNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = context.Saga.SessionId,
                    Props = (context.Saga.ProgressCurrent = 90).ToString() + ";false"
                })
            //Обновление записей аукционов (если есть) в сервисе SearchService
            .IfElse(context => JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Any(p => p.DataType == "AuctionItem"),
                p => p
                .Send(
                new Uri(configuration["QueuePaths:SearchConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "AuctionItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.EventSourcing.NotifyRestoreSnapShot"
                }),
                p => p
                .Publish(context => new NotifyRestoreSnapShot
                {
                    CorrelationId = context.Message.CorrelationId
                }))
            .TransitionTo(NotifyState));
    }

    private void ConfigureNotifyState()
    {
        During(NotifyState,
        When(NotifyEvent)
            .Then(context =>
            {
                context.Saga.NotifyMessage += $", Уведомлений - {JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "NotifyItem").Count()}";
            })
            .Send(
                new Uri(configuration["QueuePaths:RestoreProgressNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = context.Saga.SessionId,
                    Props = (context.Saga.ProgressCurrent = 95).ToString() + ";false"
                })
            //Обновление записей уведомлений (если есть) в сервисе NotifyService
            .Send(
                new Uri(configuration["QueuePaths:RestoreNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "NotifyItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.EventSourcing.RestoreSnapShotESCommit",
                    Props = context.Saga.NotifyMessage
                })
            .TransitionTo(CommitState));
    }

    private void ConfigureCommitState()
    {
        During(CommitState,
        When(CommitEvent)
            .Send(
                new Uri(configuration["QueuePaths:RestoreProgressNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = context.Saga.SessionId,
                    Props = "100" + ";true"
                })
            //посылаем через Кафку в EventSourcingService - для подтверждения транзакции
            .Activity(p => p.OfType<CommitActivity>())
            .TransitionTo(CompletedState));
    }


    private void ConfigureCompletedState()
    {
        During(CompletedState,
        When(CompleteEvent).Finalize()
        );
    }



}