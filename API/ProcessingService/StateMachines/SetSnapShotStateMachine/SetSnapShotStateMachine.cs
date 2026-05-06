using System.Text.Json;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using MassTransit;
using ProcessingService.Activities.SetSnapShot;

namespace ProcessingService.StateMachines.SetSnapShotStateMachine;

public class SetSnapShotStateMachine : MassTransitStateMachine<SetSnapShotState>
{
    public State ImagesState { get; }
    public State BidState { get; }
    public State FinanceState { get; }
    public State SearchState { get; }
    public State NotifyState { get; }
    public State CommunicationState { get; }
    public State TagState { get; }
    public State PreCommitState { get; }
    public State CommitState { get; }
    public State CompleteState { get; }
    public State AbortState { get; }

    public Event<RequestSetSnapShot> RequestEvent { get; }
    public Event<ImageSetSnapShot> ImageEvent { get; }
    public Event<BidSetSnapShot> BidEvent { get; }
    public Event<FinanceSetSnapShot> FinanceEvent { get; }
    public Event<SearchSetSnapShot> SearchEvent { get; }
    public Event<NotifySetSnapShot> NotifyEvent { get; }
    public Event<CommunicationSetSnapShot> CommunicationEvent { get; }
    public Event<TagSetSnapShot> TagEvent { get; }
    public Event<SetSnapShotESCommit> CommitEvent { get; }
    public Event<NotifyUISetSnapShot> NotifyUIEvent { get; }
    public Event<BaseServiceError> FaultEvent { get; }
    public Event<Fault<ImageSetSnapShot>> FaultImageEvent { get; }
    public Event<Fault<BidSetSnapShot>> FaultBidEvent { get; }
    public Event<Fault<FinanceSetSnapShot>> FaultFinanceEvent { get; }
    public Event<Fault<SearchSetSnapShot>> FaultSearchEvent { get; }
    public Event<Fault<NotifySetSnapShot>> FaultNotifyEvent { get; }
    public Event<Fault<CommunicationSetSnapShot>> FaultCommunicationEvent { get; }
    public Event<Fault<TagSetSnapShot>> FaultTagEvent { get; }
    public Event<Fault<SetSnapShotESCommit>> FaultCommitEvent { get; }
    public Event<Fault<NotifyUISetSnapShot>> FaultNotifyUIEvent { get; }

    private IConfiguration configuration { get; }
    private object locker = new();

    public SetSnapShotStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureImagesState();
        ConfigureBidState();
        ConfigureFinanceState();
        ConfigureSearchState();
        ConfigureNotifyState();
        ConfigureCommunicationState();
        ConfigureTagState();
        ConfigurePreCommitState();
        ConfigureCommitState();
        ConfigureCompletedState();
        ConfigureAbortState();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestEvent, p => p.InsertOnInitial = true);
        Event(() => ImageEvent);
        Event(() => BidEvent);
        Event(() => FinanceEvent);
        Event(() => SearchEvent);
        Event(() => NotifyEvent);
        Event(() => CommunicationEvent);
        Event(() => CommitEvent);
        Event(() => TagEvent);
        Event(() => FaultEvent);
        Event(() => NotifyUIEvent);
        Event(() => FaultImageEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultBidEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultFinanceEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultSearchEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultNotifyEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultCommunicationEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultTagEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultCommitEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultNotifyUIEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestEvent)
            .Then(context =>
            {
                context.Saga.UserLogin = context.Message.UserLogin;
                context.Saga.NotifyMessage = "";
                context.Saga.BatchCounter = -1;
                context.Saga.AllItemsCount = 0;
                context.Saga.ProgressCurrent = 0; //Текущей прогресс в процентах
                context.Saga.ActionDate = DateTime.UtcNow;
                context.Saga.IsError = false;
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
                            title = "Создание снимка БД:",
                            message = "Начало создания...",
                            percent = context.Saga.ProgressCurrent = 5
                        })
                    })
        //запрос батчей изображений
            .Send(
                new Uri(configuration["QueuePaths:ImageSetSnapShotConsumer"]),
                context => new ESContract
                {
                    CallBackType = "Common.Contracts.EventSourcing.ImageSetSnapShot",
                    CorrelationId = context.Message.CorrelationId
                })
            .TransitionTo(ImagesState)
        );
        SetCompletedWhenFinalized();
    }

    private void ConfigureImagesState()
    {
        During(ImagesState,
        When(ImageEvent)
            // обрабатываем поступающие изображения (или части изображений)
            .If(context => context.Saga.BatchCounter == -1,
            p => p
                .Then(context =>
                {
                    //отрабатывает один раз - пишем общее количество записываемых в лог изображений
                    lock (locker)
                    {
                        context.Saga.BatchCounter = context.Message.AllItemsCount;
                        context.Saga.AllItemsCount = context.Message.AllItemsCount;
                        context.Saga.NotifyMessage += $" Изображений - {context.Message.AllItemsCount}";
                    }
                })
            )

            //здесь принимаем все сообщения от ImageServices - и полные, и части изображения,
            // пересылаем все в EventSourcingService для сохранения в логе
            .If(context => context.Message.AllItemsCount > 0,
            p => p
            //посылаем на запись изображения или его части в ES лог
                .Send(
                    new Uri(configuration["QueuePaths:SetSnapShotConsumer"]),
                        context => new DataForProcessingServicesList<string>
                        {
                            DataObjects = context.Message.DataItems.DataObjects,
                            CorrelationId = context.Saga.CorrelationId,
                            CallBackType = "Common.Contracts.EventSourcing.ImageSetSnapShot",
                            Props = context.Saga.ActionDate.ToString()
                        }
                )
            )

            //пришел ответ после добавления изображения или его части в ES лог
            //AllItemsCount == -2 - признак что пришло сообщение об обработки части изображения в EventSourcingService, 
            //обновляем счетчик прогресса
            .If(context => context.Message.AllItemsCount == -2,
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
                                message = "Сохранение изображений...",
                                percent = context.Saga.ProgressCurrent
                            })
                        })
            )

            // переходим на обработку ставок - в следующее состояние BidState - в случаях:
            // AllItemsCount == 0 - означает что нет изображений для сохранения, или
            // AllItemsCount == -1 - это обработка полного изображения и в счетчике изображений осталось
            //последнее необработанное изображение
            .If(context => context.Message.AllItemsCount == 0 ||
                (context.Saga.BatchCounter == 1 && context.Message.AllItemsCount == -1),
            p => p
                .Publish(context => new BidSetSnapShot
                {
                    CorrelationId = context.Message.CorrelationId
                })
                .TransitionTo(BidState))

            // здесь обработка признака AllItemsCount == -1:
            // ответ от EventSourcingService, что это полное изображение, уменьшаем счетчик обрадатываемых изображений
            .If(context => context.Saga.BatchCounter != 1 && context.Message.AllItemsCount == -1,
            p => p
                .Then(context =>
                {
                    lock (locker)
                    {
                        context.Saga.BatchCounter--;
                        context.Saga.ProgressCurrent = 5 + ((context.Saga.AllItemsCount - context.Saga.BatchCounter) * 85 / context.Saga.AllItemsCount);
                    }
                })
            ),

        //обрабатываем ошибки из сервиса ImageService - получение изображений
        When(FaultImageEvent)
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

    private void ConfigureBidState()
    {
        During(BidState,
        When(BidEvent)
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
                            message = "Сохранение ставок...",
                            percent = context.Saga.ProgressCurrent += 2
                        })
                    })
            //получение ставок (если есть) из сервиса BiddingService
            .Send(
                new Uri(configuration["QueuePaths:BidSetSnapShotConsumer"]),
                context => new ESContract
                {
                    CallBackType = "Common.Contracts.EventSourcing.FinanceSetSnapShot",
                    EventData = context.Saga.ActionDate.ToString(),
                    CorrelationId = context.Saga.CorrelationId
                })
            .TransitionTo(FinanceState),
        //обрабатываем ошибки итоговой загрузки из сервиса ImageService
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
                context.Saga.NotifyMessage += $", Ставок - {context.Message.DataItems.DataObjects.Count()}";
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
                            message = "Сохранение платежей...",
                            percent = context.Saga.ProgressCurrent += 2
                        })
                    })
            //получение записей финансов (если есть) из сервиса FinanceService
            .Send(
                new Uri(configuration["QueuePaths:FinanceSetSnapShotConsumer"]),
                context => new ESContract
                {
                    CallBackType = "Common.Contracts.EventSourcing.SearchSetSnapShot",
                    EventData = context.Saga.ActionDate.ToString(),
                    CorrelationId = context.Saga.CorrelationId
                })
            .TransitionTo(SearchState),
        //обрабатываем ошибки из сервиса EventSourcingService - сохранение ставок в EsLog
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
                context.Saga.NotifyMessage += $", Платежей - {context.Message.DataItems.DataObjects.Count()}";
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
                            message = "Сохранение записей аукционов...",
                            percent = context.Saga.ProgressCurrent += 2
                        })
                    })
            //получение записей аукционов из сервиса SearchService
            .Send(
                new Uri(configuration["QueuePaths:SearchSetSnapShotConsumer"]),
                context => new ESContract
                {
                    CallBackType = "Common.Contracts.EventSourcing.NotifySetSnapShot",
                    EventData = context.Saga.ActionDate.ToString(),
                    CorrelationId = context.Saga.CorrelationId
                })
            .TransitionTo(NotifyState),
        //обрабатываем ошибки из сервиса EventSourcingService 
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
                context.Saga.NotifyMessage += $", Аукционов - {context.Message.DataItems.DataObjects.Count()}";
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
                            message = "Сохранение уведомлений...",
                            percent = context.Saga.ProgressCurrent += 2
                        })
                    })
            //Обновление записей уведомлений (если есть) в сервисе NotifyService
            .Send(
                new Uri(configuration["QueuePaths:NotifySetSnapShotConsumer"]),
                context => new ESContract
                {
                    CallBackType = "Common.Contracts.EventSourcing.CommunicationSetSnapShot",
                    EventData = context.Saga.ActionDate.ToString(),
                    CorrelationId = context.Saga.CorrelationId
                })
            .TransitionTo(CommunicationState),
        //обрабатываем ошибки из сервиса NotificationService
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
                context.Saga.NotifyMessage += $", Уведомлений - {context.Message.DataItems.DataObjects.Count()}";
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
                            message = "Сохранение сообщений пользователей...",
                            percent = context.Saga.ProgressCurrent += 2
                        })
                    })
            //Обновление записей уведомлений (если есть) в сервисе CommunicationService
            .Send(
                new Uri(configuration["QueuePaths:CommunicationsSetSnapShotConsumer"]),
                context => new ESContract
                {
                    CallBackType = "Common.Contracts.EventSourcing.TagSetSnapShot",
                    EventData = context.Saga.ActionDate.ToString(),
                    CorrelationId = context.Saga.CorrelationId
                })
            .TransitionTo(TagState),
        //обрабатываем ошибки из сервиса CommunicationService 
        When(FaultCommunicationEvent)
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
                context.Saga.NotifyMessage += $", Сообщений пользователей - {context.Message.DataItems.DataObjects.Count()}";
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
                            message = "Сохранение тегов...",
                            percent = context.Saga.ProgressCurrent += 1
                        })
                    })
            //получение записей аукционов из сервиса TagService
            .Send(
                new Uri(configuration["QueuePaths:TagSetSnapShotConsumer"]),
                context => new ESContract
                {
                    CallBackType = "Common.Contracts.EventSourcing.SetSnapShotESCommit",
                    EventData = context.Saga.ActionDate.ToString(),
                    CorrelationId = context.Saga.CorrelationId
                })
            .TransitionTo(PreCommitState),
        When(FaultTagEvent)
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
        - Fault<SetSnapShotESCommit> - событие ошибки предыдущего этапа
        - SetSnapShotESCommit - событие правильного выполнения предыдущего этапа
        на выходе - событие для подтверждения/отката транзакции - SetSnapShotESCommit
        */

    private void ConfigurePreCommitState()
    {
        During(PreCommitState,
        When(CommitEvent)
        .Then(context =>
        {
            context.Saga.NotifyMessage += $", Тегов - {context.Message.DataItems.DataObjects.Count()}";
        })
        .Publish(context => new SetSnapShotESCommit
        {
            CorrelationId = context.Saga.CorrelationId
        })
        .TransitionTo(CommitState),
        When(FaultCommitEvent)
            .Then(p => p.Saga.IsError = true)
            .Publish(context => new SetSnapShotESCommit
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
            .Publish(context => new SetSnapShotESCommit
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
            //подтверждаем/откатываем транзакцию
            .Activity(p => p.OfType<CommitActivity>())
            .TransitionTo(CompleteState)
        );
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
                //Создаем событие в сервис NotificationService для обновления интерфейса
                .Send(
                    new Uri(configuration["QueuePaths:EventNotificationConsumer"]),
                    context => new EventNotificationItem
                    {
                        SignalRMethod = SignalRMethod.SetSnapShot,
                        Show = true,
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