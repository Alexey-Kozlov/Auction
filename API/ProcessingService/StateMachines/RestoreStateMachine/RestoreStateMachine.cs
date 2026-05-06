using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Bid;
using Common.Contracts.Communication;
using Common.Contracts.ELKSearch;
using Common.Contracts.EventSourcing;
using Common.Contracts.Finance;
using Common.Contracts.Image;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using Common.Contracts.Tag;
using MassTransit;
using ProcessingService.Activities.Restore;

namespace ProcessingService.StateMachines.RestoreStateMachine;

public class RestoreStateMachine : MassTransitStateMachine<RestoreState>
{
    public State ResetItemsState { get; }
    public State StopFinishServiceState { get; }
    public State GetImagesState { get; }
    public State BidState { get; }
    public State FinanceState { get; }
    public State SearchState { get; }
    public State NotifyState { get; }
    public State CommunicationState { get; }
    public State TagState { get; }
    public State PreCommitState { get; }
    public State CommitState { get; }
    public State StartFinishServiceState { get; }
    public State ReIndexState { get; }
    public State CompleteState { get; }
    public State AbortState { get; }


    public Event<RequestRestoreItems> RequestEvent { get; }
    public Event<SendStopFinishService> StopFinishServiceEvent { get; }
    public Event<ResetItems> ResetItemsEvent { get; }
    public Event<ESLogRestoreImages> EsLogImagesEvent { get; }
    public Event<ESLogProcessImages> ProcessImagesEvent { get; }
    public Event<BidRestoreSnapShot> BidEvent { get; }
    public Event<FinanceRestoreSnapShot> FinanceEvent { get; }
    public Event<SearchRestoreSnapShot> SearchEvent { get; }
    public Event<NotifyRestoreSnapShot> NotifyEvent { get; }
    public Event<CommunicationRestoreSnapShot> CommunicationEvent { get; }
    public Event<TagRestoreSnapShot> TagEvent { get; }
    public Event<RestoreSnapShotESCommit> CommitEvent { get; }
    public Event<NotifyUIRestoreSnapShot> NotifyUIEvent { get; }
    public Event<ReIndex> ReIndexEvent { get; }
    public Event<SendStartFinishService> StartFinishServiceEvent { get; }
    public Event<BaseServiceError> FaultEvent { get; }
    public Event<Fault<SendStartFinishService>> FaultStartFinishServiceEvent { get; }
    public Event<Fault<ResetItems>> FaultResetItemsEvent { get; }
    public Event<Fault<ESLogRestoreImages>> FaultEsLogImagesEvent { get; }
    public Event<Fault<ESLogProcessImages>> FaultProcessImagesEvent { get; }
    public Event<Fault<BidRestoreSnapShot>> FaultBidEvent { get; }
    public Event<Fault<FinanceRestoreSnapShot>> FaultFinanceEvent { get; }
    public Event<Fault<SearchRestoreSnapShot>> FaultSearchEvent { get; }
    public Event<Fault<NotifyRestoreSnapShot>> FaultNotifyEvent { get; }
    public Event<Fault<CommunicationRestoreSnapShot>> FaultCommunicationEvent { get; }
    public Event<Fault<TagRestoreSnapShot>> FaultTagEvent { get; }
    public Event<Fault<RestoreSnapShotESCommit>> FaultCommitEvent { get; }
    public Event<Fault<NotifyUIRestoreSnapShot>> FaultNotifyUIEvent { get; }
    public Event<Fault<ReIndex>> FaultReIndexEvent { get; }
    public Event<Fault<SendStopFinishService>> FaultStopFinishServiceEvent { get; }

    private IConfiguration configuration { get; }
    public object locker = new();

    public RestoreStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureStopFinishServiceState();
        ConfigureResetItemsState();
        ConfigureBidState();
        ConfigureFinanceState();
        ConfigureSearchState();
        ConfigureGetImagesState();
        ConfigureNotifyState();
        ConfigureCommunicationState();
        ConfigureTagState();
        ConfigurePreCommitState();
        ConfigureCommitState();
        ConfigureStartFinishServiceState();
        ConfigureReIndexState();
        ConfigureCompletedState();
        ConfigureAbortState();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestEvent, p => p.InsertOnInitial = true);
        Event(() => StopFinishServiceEvent);
        Event(() => ResetItemsEvent);
        Event(() => EsLogImagesEvent);
        Event(() => ProcessImagesEvent);
        Event(() => BidEvent);
        Event(() => FinanceEvent);
        Event(() => SearchEvent);
        Event(() => NotifyEvent);
        Event(() => CommunicationEvent);
        Event(() => TagEvent);
        Event(() => StartFinishServiceEvent);
        Event(() => ReIndexEvent);
        Event(() => CommitEvent);
        Event(() => FaultEvent);
        Event(() => FaultStopFinishServiceEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultResetItemsEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultEsLogImagesEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultProcessImagesEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultBidEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultFinanceEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultSearchEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultNotifyEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultCommitEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultCommunicationEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultTagEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultNotifyUIEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultReIndexEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultStartFinishServiceEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestEvent)
            .Then(context =>
            {
                context.Saga.RestoreDate = context.Message.RestoreDate;
                context.Saga.UserLogin = context.Message.UserLogin;
                context.Saga.NotifyMessage = "Восстановление данных завершено";
                context.Saga.ProgressCurrent = 0; //Текущей прогресс в процентах
                context.Saga.ResetLog = context.Message.ResetLog;
                context.Saga.AllItemsCount = -1;
                context.Saga.ItemsCount = 0;
                context.Saga.IsError = false;
                context.Saga.CommitCounter = 7; //количество коллекций для сброса данных
            })
            //прогресс выполнения операции
            .Send(
                new Uri(configuration["QueuePaths:EventNotificationConsumer"]),
                    context => new EventNotificationItem
                    {
                        SignalRMethod = SignalRMethod.OperationProgress,
                        Show = true,
                        EventType = EventType.UserLogin,
                        UserLogin = context.Saga.UserLogin,
                        Data = JsonSerializer.Serialize(new
                        {
                            title = "Восстановление снимка БД:",
                            message = "Останавливаем сервис завершения аукционов...",
                            percent = context.Saga.ProgressCurrent = 1
                        })
                    })
            // посылаем сообщение на остановку сервиса проверки завершения аукционов - CheckAuctionFinished
            // причина - для недопущения некорректных ситуаций - когда начинается обработка завершенных аукционов
            // до окончания процесса восстановления
            .Send(
                    new Uri(configuration["QueuePaths:StopFinishServiceConsumer"]),
                    context => new StopFinishService
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        CallBackType = "Common.Contracts.EventSourcing.SendStopFinishService"
                    })
            .TransitionTo(StopFinishServiceState)
        );
        SetCompletedWhenFinalized();
    }

    private void ConfigureStopFinishServiceState()
    {
        During(StopFinishServiceState,
        When(StopFinishServiceEvent)
            //прогресс выполнения операции
            .Send(
                new Uri(configuration["QueuePaths:EventNotificationConsumer"]),
                    context => new EventNotificationItem
                    {
                        SignalRMethod = SignalRMethod.OperationProgress,
                        Show = true,
                        EventType = EventType.UserLogin,
                        UserLogin = context.Saga.UserLogin,
                        Data = JsonSerializer.Serialize(new
                        {
                            message = "Очищаем данные во всех 7 базах чтения...",
                            percent = context.Saga.ProgressCurrent = 5
                        })
                    })
            // посылаем через Rebbit - подготовка для удаления всех записей в 
            // BiddingService,FinanceService,NotificationService,SearchService,ImageService,
            // CommunicationService, TagService
            .Activity(p => p.OfType<ItemsResetActivity>())
            .TransitionTo(ResetItemsState),
        When(FaultStopFinishServiceEvent)
            .Then(p => p.Saga.IsError = true)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionStack = context.Message.Message.ErrorExceptionStack,
                ErrorExceptionInputData = context.Message.Message.ErrorExceptionInputData,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
        .TransitionTo(AbortState));
    }

    private void ConfigureResetItemsState()
    {
        //ожидаем подтверждения от всех сервисов об обновлении записей - подготовка к удалению
        During(ResetItemsState,
        When(ResetItemsEvent)
            .Then(context =>
            {
                lock (locker)
                {
                    context.Saga.CommitCounter--;
                }
            })
            .If(context => context.Saga.CommitCounter == 0,
            p => p
            //получили все успешные сообщения об удалении всех записей из соответствующих сервисов -
            //прогресс выполнения операции
                .Send(
                    new Uri(configuration["QueuePaths:EventNotificationConsumer"]),
                    context => new EventNotificationItem
                    {
                        SignalRMethod = SignalRMethod.OperationProgress,
                        Show = true,
                        EventType = EventType.UserLogin,
                        UserLogin = context.Saga.UserLogin,
                        Data = JsonSerializer.Serialize(new
                        {
                            message = "Получаем список аукционов...",
                            percent = context.Saga.ProgressCurrent = 8
                        })
                    })

                // посылаем через Кафку - получение из ES лог всех записей (кроме изображений) 
                // по восстановлению БД для сервисов BiddingService,FinanceService,NotificationService,
                // SearchService,CommunicationService
                // для восстановления изображений для сервиса ImageServices - ниже
                .Activity(p => p.OfType<ESLogActivityGetRecords>())
                .TransitionTo(GetImagesState)),
        When(FaultResetItemsEvent)
            .Then(p => p.Saga.IsError = true)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionStack = context.Message.Message.ErrorExceptionStack,
                ErrorExceptionInputData = context.Message.Message.ErrorExceptionInputData,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
            .TransitionTo(AbortState)
        );
    }

    private void ConfigureGetImagesState()
    {
        During(GetImagesState,
        When(EsLogImagesEvent)
            //если - DataObjects.Count() == 0 -
            //записей аукционов не найдено, делаем уведомление и завершаем процесс
            //сообщение для отслеживания прогресса, передаем признак "-1" что ничего не найдено
            .If(context => context.Message.BatchCount == -1 &&
                context.Message.DataItems.DataObjects.Count() == 0,
                p => p
                //прогресс выполнения операции
                .Send(
                    new Uri(configuration["QueuePaths:EventNotificationConsumer"]),
                    context => new EventNotificationItem
                    {
                        SignalRMethod = SignalRMethod.OperationProgress,
                        Show = true,
                        EventType = EventType.UserLogin,
                        UserLogin = context.Saga.UserLogin,
                        Data = JsonSerializer.Serialize(new
                        {
                            message = "Записей для восстановления не найдено",
                            percent = context.Saga.ProgressCurrent = 100
                        })
                    }).Finalize()
            )

            // вызываем активити ESLogActivityGetImages - после получения всех записей аукционов,
            // для получения записей изображений
            // BatchCount == -1 - признак получения текстовых записей аукционов
            .If(context => context.Message.BatchCount == -1 &&
                context.Message.DataItems.DataObjects.Count() > 0,
                p => p
                .Then(q =>
                {
                    if (q.Message.DataItems.DataObjects.Count() > 0)
                    {
                        lock (locker)
                        {
                            q.Saga.ProgressCurrent = 10;
                        }
                        //пришел набор записей (кроме изображений), сохраняем набор в ListItems
                        q.Saga.DataForProcessingServicesList = JsonSerializer.Serialize(q.Message.DataItems);
                    }
                })
                .Activity(p => p.OfType<ESLogActivityGetImages>())
            )


            // BatchCount != -1 - признак получения записей изображений
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
                // Посылаем сообщения в ImageService для сборки из отдельных частей в целое изображение
                .Send(
                new Uri(configuration["QueuePaths:ImageRestoreConsumer"]),
                    context => new DataForProcessingServicesList<ImageDTO>
                    {
                        DataObjects = context.Message.DataItems.DataObjects,
                        CorrelationId = context.Saga.CorrelationId,
                        CallBackType = "Common.Contracts.Processing.ESLogProcessImages"
                    }
                )
            ),
        //отлавливаем сообщения окончания обработки изображений - увеличиваем каждый раз счетчик изображений
        When(ProcessImagesEvent)
            .Then(context =>
            {
                lock (locker)
                {
                    context.Saga.ItemsCount++;
                    context.Saga.ProgressCurrent = 10 + context.Saga.ItemsCount * 80 / context.Saga.AllItemsCount;
                }
            })
            //прогресс выполнения операции
            .Send(
                new Uri(configuration["QueuePaths:EventNotificationConsumer"]),
                    context => new EventNotificationItem
                    {
                        SignalRMethod = SignalRMethod.OperationProgress,
                        Show = true,
                        EventType = EventType.UserLogin,
                        UserLogin = context.Saga.UserLogin,
                        Data = JsonSerializer.Serialize(new
                        {
                            message = "Восстановление изображений...",
                            percent = context.Saga.ProgressCurrent
                        })
                    })
            //конец обработки изображений - переходим на следующий этап
            .If(context => context.Saga.AllItemsCount == context.Saga.ItemsCount,
                p => p
                .Publish(context => new BidRestoreSnapShot
                {
                    CorrelationId = context.Message.CorrelationId
                })
            .TransitionTo(BidState)),
        //ошибки обработки получения изображений из EsLog
        When(FaultEsLogImagesEvent)
            .Then(p => p.Saga.IsError = true)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionStack = context.Message.Message.ErrorExceptionStack,
                ErrorExceptionInputData = context.Message.Message.ErrorExceptionInputData,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
        .TransitionTo(AbortState),
        //ошибки обработки изображений при записи в таблицу в сервисе ImageService
        When(FaultProcessImagesEvent)
            .Then(p => p.Saga.IsError = true)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionStack = context.Message.Message.ErrorExceptionStack,
                ErrorExceptionInputData = context.Message.Message.ErrorExceptionInputData,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
        .TransitionTo(PreCommitState)
        );
    }
    private void ConfigureBidState()
    {
        During(BidState,
        When(BidEvent)
            .Then(context =>
            {
                context.Saga.NotifyMessage += $", Ставок - {JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "BidItem").Count()}";
                context.Saga.NotifyMessage += $", Изображений - {context.Saga.ItemsCount}";
            })
            //прогресс выполнения операции
            .Send(
                new Uri(configuration["QueuePaths:EventNotificationConsumer"]),
                    context => new EventNotificationItem
                    {
                        SignalRMethod = SignalRMethod.OperationProgress,
                        Show = true,
                        EventType = EventType.UserLogin,
                        UserLogin = context.Saga.UserLogin,
                        Data = JsonSerializer.Serialize(new
                        {
                            message = "Восстановление ставок аукционов...",
                            percent = context.Saga.ProgressCurrent = 91
                        })
                    })
            //Обновление ставок (если есть) в сервисе BiddingService
            .Send(
                new Uri(configuration["QueuePaths:BidConsumer"]),
                context => new DataForProcessingServicesList<BidItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "BidItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.EventSourcing.FinanceRestoreSnapShot"
                })
            .TransitionTo(FinanceState),
        When(FaultBidEvent)
            .Then(p => p.Saga.IsError = true)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionStack = context.Message.Message.ErrorExceptionStack,
                ErrorExceptionInputData = context.Message.Message.ErrorExceptionInputData,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
        .TransitionTo(PreCommitState)
        );
    }

    private void ConfigureFinanceState()
    {
        During(FinanceState,
        When(FinanceEvent)
            .Then(context =>
            {
                context.Saga.NotifyMessage += $", Финансов - {JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "FinanceItem").Count()}";
            })
            //прогресс выполнения операции
            .Send(
                new Uri(configuration["QueuePaths:EventNotificationConsumer"]),
                    context => new EventNotificationItem
                    {
                        SignalRMethod = SignalRMethod.OperationProgress,
                        Show = true,
                        EventType = EventType.UserLogin,
                        UserLogin = context.Saga.UserLogin,
                        Data = JsonSerializer.Serialize(new
                        {
                            message = "Восстановление записей финансов...",
                            percent = context.Saga.ProgressCurrent = 92
                        })
                    })
            //Обновление денег (если есть) в сервисе FinanceService
            .Send(
                new Uri(configuration["QueuePaths:FinanceConsumer"]),
                context => new DataForProcessingServicesList<FinanceItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "FinanceItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.EventSourcing.SearchRestoreSnapShot"
                })
            .TransitionTo(SearchState),
        When(FaultFinanceEvent)
            .Then(p => p.Saga.IsError = true)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionStack = context.Message.Message.ErrorExceptionStack,
                ErrorExceptionInputData = context.Message.Message.ErrorExceptionInputData,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
        .TransitionTo(PreCommitState)
        );
    }

    private void ConfigureSearchState()
    {
        During(SearchState,
        When(SearchEvent)
            .Then(context =>
            {
                context.Saga.NotifyMessage += $", Аукционов - {JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "AuctionItem").Count()}";
            })
            //прогресс выполнения операции
            .Send(
                new Uri(configuration["QueuePaths:EventNotificationConsumer"]),
                    context => new EventNotificationItem
                    {
                        SignalRMethod = SignalRMethod.OperationProgress,
                        Show = true,
                        EventType = EventType.UserLogin,
                        UserLogin = context.Saga.UserLogin,
                        Data = JsonSerializer.Serialize(new
                        {
                            message = "Восстановление записей аукционов...",
                            percent = context.Saga.ProgressCurrent = 93
                        })
                    })
            //Обновление записей аукционов (если есть) в сервисе SearchService
            .Send(
                new Uri(configuration["QueuePaths:SearchConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects
                        .Where(p => p.DataType == "AuctionItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.EventSourcing.NotifyRestoreSnapShot"
                })
            .TransitionTo(NotifyState),
        When(FaultSearchEvent)
            .Then(p => p.Saga.IsError = true)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionStack = context.Message.Message.ErrorExceptionStack,
                ErrorExceptionInputData = context.Message.Message.ErrorExceptionInputData,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
        .TransitionTo(PreCommitState)
        );
    }

    private void ConfigureNotifyState()
    {
        During(NotifyState,
        When(NotifyEvent)
            .Then(context =>
            {
                context.Saga.NotifyMessage += $", Уведомлений - {JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "NotifyItem").Count()}";
            })
            //прогресс выполнения операции
            .Send(
                new Uri(configuration["QueuePaths:EventNotificationConsumer"]),
                    context => new EventNotificationItem
                    {
                        SignalRMethod = SignalRMethod.OperationProgress,
                        Show = true,
                        EventType = EventType.UserLogin,
                        UserLogin = context.Saga.UserLogin,
                        Data = JsonSerializer.Serialize(new
                        {
                            message = "Восстановление записей уведомлений...",
                            percent = context.Saga.ProgressCurrent = 94
                        })
                    })
            //Обновление записей уведомлений (если есть) в сервисе NotifyService
            .Send(
                new Uri(configuration["QueuePaths:RestoreNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "NotifyItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.EventSourcing.CommunicationRestoreSnapShot",
                    Props = context.Saga.NotifyMessage
                })
            .TransitionTo(CommunicationState),
        When(FaultNotifyEvent)
            .Then(p => p.Saga.IsError = true)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionStack = context.Message.Message.ErrorExceptionStack,
                ErrorExceptionInputData = context.Message.Message.ErrorExceptionInputData,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
            .TransitionTo(PreCommitState)
        );
    }

    private void ConfigureCommunicationState()
    {
        During(CommunicationState,
        When(CommunicationEvent)
            .Then(context =>
            {
                context.Saga.NotifyMessage += $", Сообщений пользователей - {JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "CommunicationItem").Count()}";
            })
            //прогресс выполнения операции
            .Send(
                new Uri(configuration["QueuePaths:EventNotificationConsumer"]),
                    context => new EventNotificationItem
                    {
                        SignalRMethod = SignalRMethod.OperationProgress,
                        Show = true,
                        EventType = EventType.UserLogin,
                        UserLogin = context.Saga.UserLogin,
                        Data = JsonSerializer.Serialize(new
                        {
                            message = "Восстановление записей сообщений пользователей...",
                            percent = context.Saga.ProgressCurrent = 95
                        })
                    })
            //Обновление записей уведомлений (если есть) в сервисе CommunicationService
            .Send(
                new Uri(configuration["QueuePaths:RestoreCommunicationConsumer"]),
                context => new DataForProcessingServicesList<CommunicationItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "CommunicationItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.EventSourcing.TagRestoreSnapShot",
                    Props = context.Saga.NotifyMessage
                })
            .TransitionTo(TagState),
        When(FaultNotifyEvent)
            .Then(p => p.Saga.IsError = true)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionStack = context.Message.Message.ErrorExceptionStack,
                ErrorExceptionInputData = context.Message.Message.ErrorExceptionInputData,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
            .TransitionTo(PreCommitState)
        );
    }

    private void ConfigureTagState()
    {
        During(TagState,
        When(TagEvent)
            .Then(context =>
            {
                context.Saga.NotifyMessage += $", Тегов - {JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "TagItem").Count()}";
            })
            //прогресс выполнения операции
            .Send(
                new Uri(configuration["QueuePaths:EventNotificationConsumer"]),
                    context => new EventNotificationItem
                    {
                        SignalRMethod = SignalRMethod.OperationProgress,
                        Show = true,
                        EventType = EventType.UserLogin,
                        UserLogin = context.Saga.UserLogin,
                        Data = JsonSerializer.Serialize(new
                        {
                            message = "Восстановление записей тегов...",
                            percent = context.Saga.ProgressCurrent = 96
                        })
                    })
            //Обновление записей тегов (если есть) в сервисе TagService
            .Send(
                new Uri(configuration["QueuePaths:RestoreTagConsumer"]),
                context => new DataForProcessingServicesList<TagItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "TagItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Processing.ReIndex",
                    Props = context.Saga.NotifyMessage
                })
            .TransitionTo(ReIndexState),
        When(FaultNotifyEvent)
            .Then(p => p.Saga.IsError = true)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionStack = context.Message.Message.ErrorExceptionStack,
                ErrorExceptionInputData = context.Message.Message.ErrorExceptionInputData,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
            .TransitionTo(PreCommitState)
        );
    }


    private void ConfigureReIndexState()
    {
        During(ReIndexState,
        When(ReIndexEvent)
            //прогресс выполнения операции
            .Send(
                new Uri(configuration["QueuePaths:EventNotificationConsumer"]),
                    context => new EventNotificationItem
                    {
                        SignalRMethod = SignalRMethod.OperationProgress,
                        Show = true,
                        EventType = EventType.UserLogin,
                        UserLogin = context.Saga.UserLogin,
                        Data = JsonSerializer.Serialize(new
                        {
                            message = "Переиндексация...",
                            percent = context.Saga.ProgressCurrent = 96
                        })
                    })
            // Переиндексация
            .Publish(context => new ElkIndexRequest
            {
                CorrelationId = context.Saga.CorrelationId,
                UserLogin = context.Saga.UserLogin,
                ShowMessages = false,
                CallBackType = "Common.Contracts.EventSourcing.RestoreSnapShotESCommit"
            })
            .TransitionTo(PreCommitState),
        When(FaultReIndexEvent)
            .Then(p => p.Saga.IsError = true)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionStack = context.Message.Message.ErrorExceptionStack,
                ErrorExceptionInputData = context.Message.Message.ErrorExceptionInputData,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
            .TransitionTo(PreCommitState)
        );
    }


    /*промежуточный этап перед подтверждением/откатом транзакции
       на входе события:
       - BaseServiceError - событие ошибок от предыдущих этапов
       - Fault<RestoreSnapShotESCommit> - событие ошибки предыдущего этапа
       - RestoreSnapShotESCommit - событие правильного выполнения предыдущего этапа
       на выходе - событие для подтверждения/отката транзакции - RestoreSnapShotESCommit
       */

    private void ConfigurePreCommitState()
    {
        During(PreCommitState,
        When(CommitEvent)
            .Publish(context => new RestoreSnapShotESCommit
            {
                CorrelationId = context.Saga.CorrelationId
            })
        .TransitionTo(CommitState),
        When(FaultCommitEvent)
            .Then(p => p.Saga.IsError = true)
            .Publish(context => new RestoreSnapShotESCommit
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionStack = context.Message.Message.ErrorExceptionStack,
                ErrorExceptionInputData = context.Message.Message.ErrorExceptionInputData,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Message.Message.UserLogin
            })
        .TransitionTo(CommitState),
        //обработка ошибок - передаем отмену коммита и инфу по ошибке пользователю в UI
        When(FaultEvent)
            .Publish(context => new RestoreSnapShotESCommit
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.ErrorMessage,
                ErrorExceptionStack = context.Message.ErrorExceptionStack,
                ErrorExceptionInputData = context.Message.ErrorExceptionInputData,
                ErrorServiceName = context.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
        .TransitionTo(CommitState)
        );
    }


    private void ConfigureCommitState()
    {
        During(CommitState,
        When(CommitEvent)
            .Then(context => context.Saga.CommitCounter = 7)
            //прогресс выполнения операции
            .Send(
                new Uri(configuration["QueuePaths:EventNotificationConsumer"]),
                    context => new EventNotificationItem
                    {
                        SignalRMethod = SignalRMethod.OperationProgress,
                        Show = !context.Saga.IsError,
                        EventType = EventType.UserLogin,
                        UserLogin = context.Saga.UserLogin,
                        Data = JsonSerializer.Serialize(new
                        {
                            message = "Фиксация транзакции восстановления...",
                            percent = context.Saga.ProgressCurrent = 97
                        })
                    })
            // посылаем через Кафку в EventSourcingService - для подтверждения транзакции
            // фиксируем изменения во всех базах данных чтения
            .Activity(p => p.OfType<CommitActivity>())
            .TransitionTo(StartFinishServiceState));
    }

    private void ConfigureStartFinishServiceState()
    {
        During(StartFinishServiceState,
        When(StartFinishServiceEvent)
            //ждем сообщений о завершении транзакции всех 5 коллекций с записями
            .Then(context =>
            {
                lock (locker)
                {
                    context.Saga.CommitCounter--;
                }
            })
            .If(context => context.Saga.CommitCounter == 0,
            r => r
            //транзакции завершены, обрабатываем возможные ошибки
                .IfElse(context => context.Saga.IsError,
                p => p
                //в процессе выполнения произошла ошибка
                .Send(
                    new Uri(configuration["QueuePaths:ErrorNotificationConsumer"]),
                    context => new NotificationServiceError
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        ErrorMessage = context.Message.ErrorMessage,
                        ErrorExceptionStack = context.Message.ErrorExceptionStack,
                        ErrorExceptionInputData = context.Message.ErrorExceptionInputData,
                        ErrorServiceName = context.Message.ErrorServiceName,
                        UserLogin = context.Saga.UserLogin,
                        TraceId = Guid.NewGuid(),
                    }).Finalize(),
                p => p
                //все прошло корректно, ошибок нет
                //прогресс выполнения операции
                .Send(
                    new Uri(configuration["QueuePaths:EventNotificationConsumer"]),
                    context => new EventNotificationItem
                    {
                        SignalRMethod = SignalRMethod.OperationProgress,
                        Show = !context.Saga.IsError,
                        EventType = EventType.UserLogin,
                        UserLogin = context.Saga.UserLogin,
                        Data = JsonSerializer.Serialize(new
                        {
                            message = "Запускаем сервис проверки завершеия аукционов...",
                            percent = context.Saga.ProgressCurrent = 98
                        })
                    })
                    // запускаем сервис завершения аукционов 
                    .Send(
                        new Uri(configuration["QueuePaths:StartFinishServiceConsumer"]),
                        context => new StartFinishService
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            CallBackType = "Common.Contracts.EventSourcing.NotifyUIRestoreSnapShot"
                        })
            .TransitionTo(CompleteState))),
        When(FaultStartFinishServiceEvent)
            .Then(p => p.Saga.IsError = true)
            .Send(
                new Uri(configuration["QueuePaths:ErrorNotificationConsumer"]),
                context => new NotificationServiceError
                {
                    CorrelationId = context.Saga.CorrelationId,
                    ErrorMessage = context.Message.Message.ErrorMessage,
                    ErrorExceptionStack = context.Message.Message.ErrorExceptionStack,
                    ErrorExceptionInputData = context.Message.Message.ErrorExceptionInputData,
                    ErrorServiceName = context.Message.Message.ErrorServiceName,
                    UserLogin = context.Saga.UserLogin,
                    TraceId = Guid.NewGuid(),
                })
            .Finalize());
    }

    private void ConfigureCompletedState()
    {
        During(CompleteState,
        When(NotifyUIEvent)
            .IfElse(context => context.Saga.IsError,
            p => p
            //в процессе выполнения произошла ошибка
            .Send(
                new Uri(configuration["QueuePaths:ErrorNotificationConsumer"]),
                context => new NotificationServiceError
                {
                    CorrelationId = context.Saga.CorrelationId,
                    ErrorMessage = context.Message.ErrorMessage,
                    ErrorExceptionStack = context.Message.ErrorExceptionStack,
                    ErrorExceptionInputData = context.Message.ErrorExceptionInputData,
                    ErrorServiceName = context.Message.ErrorServiceName,
                    UserLogin = context.Saga.UserLogin,
                    TraceId = Guid.NewGuid(),
                }).Finalize(),
            p => p
            //посылаем финальное сообщение для вывода сообщения об итогах восстановления
                .Send(
                    new Uri(configuration["QueuePaths:EventNotificationConsumer"]),
                    context => new EventNotificationItem
                    {
                        SignalRMethod = SignalRMethod.RestoreSnapShot,
                        Show = !context.Saga.IsError,
                        EventType = EventType.UserLogin,
                        UserLogin = context.Saga.UserLogin,
                        Data = context.Saga.NotifyMessage
                    }).Finalize()
            ),
        When(FaultNotifyUIEvent)
        .Send(
            new Uri(configuration["QueuePaths:ErrorNotificationConsumer"]),
            context => new NotificationServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionStack = context.Message.Message.ErrorExceptionStack,
                ErrorExceptionInputData = context.Message.Message.ErrorExceptionInputData,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin,
                TraceId = Guid.NewGuid(),
            })
            .Finalize()
        );
    }

    private void ConfigureAbortState()
    {
        During(AbortState,
        When(FaultEvent)
        .Send(
            new Uri(configuration["QueuePaths:ErrorNotificationConsumer"]),
            context => new NotificationServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.ErrorMessage,
                ErrorExceptionStack = context.Message.ErrorExceptionStack,
                ErrorExceptionInputData = context.Message.ErrorExceptionInputData,
                ErrorServiceName = context.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin,
                TraceId = Guid.NewGuid(),
            })
        .Finalize());
    }
}