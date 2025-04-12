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
    public State ResetItemsState { get; }
    public State GetImagesState { get; }
    public State BidState { get; }
    public State FinanceState { get; }
    public State SearchState { get; }
    public State NotifyState { get; }
    public State PreCommitState { get; }
    public State CommitState { get; }
    public State CompleteState { get; }


    public Event<RequestRestoreItems> RequestEvent { get; }
    public Event<ResetItems> ResetItemsEvent { get; }
    public Event<ESLogRestoreImages> EsLogImagesEvent { get; }
    public Event<ESLogProcessImages> ProcessImagesEvent { get; }
    public Event<BidRestoreSnapShot> BidEvent { get; }
    public Event<FinanceRestoreSnapShot> FinanceEvent { get; }
    public Event<SearchRestoreSnapShot> SearchEvent { get; }
    public Event<NotifyRestoreSnapShot> NotifyEvent { get; }
    public Event<RestoreSnapShotESCommit> CommitEvent { get; }
    public Event<NotifyUIRestoreSnapShot> NotifyUIEvent { get; }
    public Event<BaseServiceError> FaultEvent { get; }
    public Event<Fault<ResetItems>> FaultResetItemsEvent { get; }
    public Event<Fault<ESLogRestoreImages>> FaultEsLogImagesEvent { get; }
    public Event<Fault<ESLogProcessImages>> FaultProcessImagesEvent { get; }
    public Event<Fault<BidRestoreSnapShot>> FaultBidEvent { get; }
    public Event<Fault<FinanceRestoreSnapShot>> FaultFinanceEvent { get; }
    public Event<Fault<SearchRestoreSnapShot>> FaultSearchEvent { get; }
    public Event<Fault<NotifyRestoreSnapShot>> FaultNotifyEvent { get; }
    public Event<Fault<RestoreSnapShotESCommit>> FaultCommitEvent { get; }
    public Event<Fault<NotifyUIRestoreSnapShot>> FaultNotifyUIEvent { get; }

    private IConfiguration configuration { get; }
    public object locker = new();

    public RestoreStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureResetItemsState();
        ConfigureBidState();
        ConfigureFinanceState();
        ConfigureSearchState();
        ConfigureGetImagesState();
        ConfigureNotifyState();
        ConfigurePreCommitState();
        ConfigureCommitState();
        ConfigureCompletedState();

    }
    private void ConfigureEvents()
    {
        Event(() => RequestEvent, p => p.InsertOnInitial = true);
        Event(() => ResetItemsEvent);
        Event(() => EsLogImagesEvent);
        Event(() => ProcessImagesEvent);
        Event(() => BidEvent);
        Event(() => FinanceEvent);
        Event(() => SearchEvent);
        Event(() => NotifyEvent);
        Event(() => CommitEvent);
        Event(() => FaultEvent);
        Event(() => FaultResetItemsEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultEsLogImagesEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultProcessImagesEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultBidEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultFinanceEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultSearchEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultNotifyEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultCommitEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultNotifyUIEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
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
                context.Saga.SessionId = context.Message.SessionId;
                context.Saga.ResetLog = context.Message.ResetLog;
                context.Saga.AllItemsCount = -1;
                context.Saga.ItemsCount = 0;
                context.Saga.IsError = false;
                context.Saga.CommitCounter = 5; //количество коллекций для сброса данных
            })
            //посылаем через Ребит - удаление всех записей в BiddingService,FinanceService,NotificationService,SearchService,ImageService
            .Activity(p => p.OfType<ItemsResetActivity>())
            .TransitionTo(ResetItemsState)
        );
        SetCompletedWhenFinalized();
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
            .TransitionTo(GetImagesState)),
        When(FaultResetItemsEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                Message = context.Message.Message.Message,
                ExceptionMessage = context.Message.Message.ExceptionMessage,
                ServiceName = context.Message.Message.ServiceName,
                UserLogin = context.Saga.UserLogin
            })
        .TransitionTo(PreCommitState));
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
                .Send(
                    new Uri(configuration["QueuePaths:RestoreProgressNotificationConsumer"]),
                    context => new DataForProcessingServicesList<NotifyItem>
                    {
                        DataObjects = new List<DataForProcessingService>(),
                        CorrelationId = context.Saga.CorrelationId,
                        CallBackType = context.Saga.SessionId,
                        Props = "-1;true"
                    })
                .Send(
                    new Uri(configuration["QueuePaths:RestoreEventNotificationConsumer"]),
                context => new ESContract
                {
                    CallBackType = "",
                    EventData = "Записей не найдено",
                    UserLogin = context.Saga.SessionId
                })
                .Finalize()
            )

            // вызываем активити ESLogActivityGetImages - после получения всех записей аукционов,
            //для получения записей изображений
            //BatchCount == -1 - признак получения текстовых записей аукционов
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
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                Message = context.Message.Message.Message,
                ExceptionMessage = context.Message.Message.ExceptionMessage,
                ServiceName = context.Message.Message.ServiceName,
                UserLogin = context.Saga.UserLogin
            })
        .TransitionTo(PreCommitState),
        //ошибки обработки изображений при записи в таблицу в сервисе ImageService
        When(FaultProcessImagesEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                Message = context.Message.Message.Message,
                ExceptionMessage = context.Message.Message.ExceptionMessage,
                ServiceName = context.Message.Message.ServiceName,
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
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                Message = context.Message.Message.Message,
                ExceptionMessage = context.Message.Message.ExceptionMessage,
                ServiceName = context.Message.Message.ServiceName,
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
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                Message = context.Message.Message.Message,
                ExceptionMessage = context.Message.Message.ExceptionMessage,
                ServiceName = context.Message.Message.ServiceName,
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
            .Send(
                new Uri(configuration["QueuePaths:SearchConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "AuctionItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.EventSourcing.NotifyRestoreSnapShot"
                })
            .TransitionTo(NotifyState),
        When(FaultSearchEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                Message = context.Message.Message.Message,
                ExceptionMessage = context.Message.Message.ExceptionMessage,
                ServiceName = context.Message.Message.ServiceName,
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
            .TransitionTo(PreCommitState),
        When(FaultNotifyEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                Message = context.Message.Message.Message,
                ExceptionMessage = context.Message.Message.ExceptionMessage,
                ServiceName = context.Message.Message.ServiceName,
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
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new RestoreSnapShotESCommit
            {
                CorrelationId = context.Saga.CorrelationId,
                Message = context.Message.Message.Message,
                ExceptionMessage = context.Message.Message.ExceptionMessage,
                ServiceName = context.Message.Message.ServiceName,
                UserLogin = context.Message.Message.UserLogin
            })
        .TransitionTo(CommitState),
        //обработка ошибок - передаем отмену коммита и инфу по ошибке пользователю в UI
        When(FaultEvent)
            .Publish(context => new RestoreSnapShotESCommit
            {
                CorrelationId = context.Saga.CorrelationId,
                Message = context.Message.Message,
                ExceptionMessage = context.Message.ExceptionMessage,
                ServiceName = context.Message.ServiceName,
                UserLogin = context.Message.UserLogin
            })
        .TransitionTo(CommitState)
        );
    }


    private void ConfigureCommitState()
    {
        During(CommitState,
        When(CommitEvent)
            .Then(context => context.Saga.CommitCounter = 5)
            .Send(
                new Uri(configuration["QueuePaths:RestoreProgressNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = context.Saga.SessionId,
                    //-1 - передаем -1 - в случае не отображать уведомление
                    Props = context.Saga.IsError ? "-1;true" : "100;true"
                })
            //посылаем через Кафку в EventSourcingService - для подтверждения транзакции
            .Activity(p => p.OfType<CommitActivity>())
            .TransitionTo(CompleteState));
    }

    private void ConfigureCompletedState()
    {
        During(CompleteState,
        When(NotifyUIEvent)
            //ждем сообщений об обработке всех 5 коллекций с записями
            .Then(context =>
            {
                lock (locker)
                {
                    context.Saga.CommitCounter--;
                }
            })
            .If(context => context.Saga.CommitCounter == 0,
            r => r
                .IfElse(context => context.Saga.IsError,
                p => p
                //в процессе выполнения произошла ошибка
                .Send(
                    new Uri(configuration["QueuePaths:ErrorNotificationConsumer"]),
                    context => new NotificationServiceError
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        Message = context.Message.Message,
                        ExceptionMessage = context.Message.ExceptionMessage,
                        ServiceName = context.Message.ServiceName,
                        UserLogin = context.Saga.UserLogin,
                        TraceId = Guid.NewGuid(),
                        IsError = context.Saga.IsError
                    }).Finalize(),
                p => p
                //посылаем финальное сообщение для вывода сообщеничя об итогах восстановления
                .Send(
                    new Uri(configuration["QueuePaths:RestoreEventNotificationConsumer"]),
                    context => new ESContract
                    {
                        CallBackType = "",
                        EventData = context.Saga.NotifyMessage,
                        UserLogin = context.Saga.SessionId
                    }).Finalize()
                )
            ),
        When(FaultNotifyUIEvent)
        .Send(
            new Uri(configuration["QueuePaths:ErrorNotificationConsumer"]),
            context => new NotificationServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                Message = context.Message.Message.Message,
                ExceptionMessage = context.Message.Message.ExceptionMessage,
                ServiceName = context.Message.Message.ServiceName,
                UserLogin = context.Saga.UserLogin,
                TraceId = Guid.NewGuid(),
                IsError = context.Saga.IsError
            })
            .Finalize()
        );
    }

}