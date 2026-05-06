using System.Text.Json;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using MassTransit;
using ProcessingService.Activities.EditNotification;

namespace ProcessingService.StateMachines.EditNotificationStateMachine;

public class EditNotificationStateMachine : MassTransitStateMachine<EditNotificationState>
{
    public State NotificationState { get; }
    public State CommitState { get; }
    public State PreCommitState { get; }
    public State CompletedState { get; }
    public State AbortState { get; }


    public Event<RequestEditNotification> RequestEvent { get; }
    public Event<ESLogEditNotification> EsLogEvent { get; }
    public Event<EditNotificationESCommit> CommitEvent { get; }
    public Event<EditNotificationComplete> CompleteEvent { get; }
    public Event<BaseServiceError> FaultEvent { get; }
    public Event<EditNotificationEvent> NotificationUIEvent { get; }
    public Event<Fault<ESLogEditNotification>> FaultEsLogEvent { get; }
    public Event<Fault<EditNotificationESCommit>> FaultCommitEvent { get; }
    public Event<Fault<EditNotificationEvent>> FaultNotificationUIEvent { get; }
    private IConfiguration configuration { get; }
    public object locker = new();


    public EditNotificationStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureNotificationState();
        ConfigurePreCommitState();
        ConfigureCommitState();
        ConfigureCompleted();
        ConfigureAbortState();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestEvent, p =>
        {
            p.InsertOnInitial = true;
        });
        Event(() => EsLogEvent);
        Event(() => CommitEvent);
        Event(() => CompleteEvent);
        Event(() => FaultEvent);
        Event(() => FaultEsLogEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultCommitEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestEvent)
            .Then(context =>
            {
                context.Saga.ItemId = context.Message.ItemId;
                context.Saga.Enable = context.Message.Enable;
                context.Saga.UserLogin = context.Message.UserLogin;
                context.Saga.IsError = false;
                context.Saga.CommitCounter = 2;
            })
            //посылаем через Кафку
            // - Создаем / удаляем уведомление для данного пользователя для данного аукциона
            // - Возвращаем NotifyItem для создания / удаления в NotificationService
            .Activity(p => p.OfType<ESLogActivity>())
            .TransitionTo(NotificationState)
        );
        SetCompletedWhenFinalized();
    }

    private void ConfigureNotificationState()
    {
        During(NotificationState,
        When(EsLogEvent)
            .Then(context =>
            {
                context.Saga.DataForProcessingServicesList = JsonSerializer.Serialize(context.Message.DataItems);
            })
            .Send(
                new Uri(configuration["QueuePaths:EditNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = context.Message.DataItems.DataObjects,
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Notification.EditNotificationESCommit",
                    Props = ""
                })
            .TransitionTo(PreCommitState),
        //обрабатываем ошибки из сервиса EventSourcingService            
        When(FaultEsLogEvent)
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

    private void ConfigurePreCommitState()
    {
        During(PreCommitState,
        When(CommitEvent)
            .Publish(context => new EditNotificationESCommit
            {
                CorrelationId = context.Saga.CorrelationId
            })
        .TransitionTo(CommitState),
        When(FaultCommitEvent)
            .Then(p => p.Saga.IsError = true)
            .Publish(context => new EditNotificationESCommit
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
            .Publish(context => new EditNotificationESCommit
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
            //посылаем через Кафку в EventSourcingService - для подтверждения транзакции
            .Activity(p => p.OfType<CommitActivity>())
            .TransitionTo(CompletedState));
    }

    private void ConfigureCompleted()
    {
        During(CompletedState,
        When(NotificationUIEvent)
            //ждем сообщений об обработке 2 коллекций с записями
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
                        SignalRMethod = SignalRMethod.EditNotification,
                        AuctionId = context.Saga.ItemId,
                        ItemId = context.Saga.ItemId,
                        Show = !context.Saga.IsError,
                        EventType = EventType.UserLogin,
                        UserLogin = context.Saga.UserLogin,
                        Data = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList)
                            .DataObjects[0].Data
                    })).Finalize()
            ),
        //обрабатываем ошибки подтверждения/отката транзакции            
        When(FaultNotificationUIEvent)
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
                AuctionId = context.Saga.ItemId,
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