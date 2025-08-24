using System.Text.Json;
using Common.Contracts.Finance;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using MassTransit;
using ProcessingService.Activities.Finance;

namespace ProcessingService.StateMachines.FinanceStateMachine;

public class FinanceStateMachine : MassTransitStateMachine<FinanceState>
{
    public State FinanceState { get; }
    public State PreCommitState { get; }
    public State CommitState { get; }
    public State CompleteState { get; }

    public Event<RequestCreateFinance> RequestEvent { get; }
    public Event<ESLogFinanceCreated> EsLogEvent { get; }
    public Event<FinanceNotificationCreated> NotificationEvent { get; }
    public Event<Fault<FinanceCreateESCommit>> FaultCommitEvent { get; }
    public Event<FinanceCreateESCommit> CommitEvent { get; }
    public Event<Fault<ESLogFinanceCreated>> FaultEsLogEvent { get; }
    public Event<Fault<FinanceNotificationCreated>> FaultNotificationEvent { get; }
    public Event<BaseServiceError> FaultEvent { get; }
    private IConfiguration configuration { get; }
    public object locker = new();


    public FinanceStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureFinanceState();
        ConfigureCompleteState();
        ConfigureCommitState();
        ConfigurePreCommitState();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestEvent, p => p.InsertOnInitial = true);
        Event(() => EsLogEvent);
        Event(() => NotificationEvent);
        Event(() => CommitEvent);
        Event(() => FaultCommitEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultEsLogEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultNotificationEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestEvent)
            .Then(context =>
            {
                context.Saga.Amount = context.Message.Amount;
                context.Saga.ItemId = Guid.NewGuid();
                context.Saga.UserLogin = context.Message.UserLogin;
                context.Saga.IsError = false;
                context.Saga.CommitCounter = 2;
            })
            //посылаем через Кафку, выполнение всех операций в ES лог для пополнения счета пользователя
            // - Добавление записей по деньгам в сервисе FinanceService
            // - Возвращаем FinanceItem и обновленный баланс
            .Activity(p => p.OfType<ESLogActivity>())
            .TransitionTo(FinanceState)
        );
        SetCompletedWhenFinalized();
    }

    private void ConfigureFinanceState()
    {
        During(FinanceState,
        When(EsLogEvent)
                //делаем рассылку для создания новой записи поступления денег и корректировки баланса в FinanceService
                .Send(
                new Uri(configuration["QueuePaths:FinanceConsumer"]),
                context => new DataForProcessingServicesList<FinanceItem>
                {
                    DataObjects = context.Message.DataItems.DataObjects,
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Finance.FinanceCreateESCommit"
                })
            .TransitionTo(PreCommitState),
        //обрабатываем ошибки из сервиса EventSourcingService            
        When(FaultEsLogEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionMessage = context.Message.Message.ErrorExceptionMessage,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
            .TransitionTo(PreCommitState)
        );
    }

    /*промежуточный этап перед подтверждением/откатом транзакции
    на входе события:
    - FinanceError - событие ошибок от предыдущих этапов
    - Fault<FinanceCreateESCommit> - событие ошибки предыдущего этапа
    - FinanceCreateESCommit - событие правильного выполнения предыдущего этапа
    на выходе - событие для подтверждения/отката транзакции - FinanceCreateESCommit
    */

    private void ConfigurePreCommitState()
    {
        During(PreCommitState,
        When(CommitEvent)
            .Publish(context => new FinanceCreateESCommit
            {
                CorrelationId = context.Saga.CorrelationId,
            })
        .TransitionTo(CommitState),
        When(FaultCommitEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new FinanceCreateESCommit
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionMessage = context.Message.Message.ErrorExceptionMessage,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Message.Message.UserLogin
            })
        .TransitionTo(CommitState),
        //обработка ошибок - передаем отмену коммита и инфу по ошибке пользователю в UI
        When(FaultEvent)
            .Publish(context => new FinanceCreateESCommit
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.ErrorMessage,
                ErrorExceptionMessage = context.Message.ErrorExceptionMessage,
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
            //посылаем через Кафку в EventSourcingService - для подтверждения/отмены транзакции
            .Activity(p => p.OfType<CommitActivity>())
            .TransitionTo(CompleteState));
    }

    private void ConfigureCompleteState()
    {
        During(CompleteState,
        When(NotificationEvent)
            //передаем сообщение для обновления UI (если не было ошибок)
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
                        ErrorMessage = context.Message.ErrorMessage,
                        ErrorExceptionMessage = context.Message.ErrorExceptionMessage,
                        ErrorServiceName = context.Message.ErrorServiceName,
                        UserLogin = context.Saga.UserLogin,
                        TraceId = Guid.NewGuid(),
                        IsError = context.Saga.IsError
                    }).Finalize(),
                p => p
                //Создаем событие в сервис NotificationService для обновления интерфейса
                .Send(
                    new Uri(configuration["QueuePaths:EventNotificationConsumer"]),
                    context => new EventNotificationItem
                    {
                        SignalRMethod = SignalRMethod.FinanceCreate,
                        Show = true,
                        EventType = EventType.UserLogin,
                        UserLogin = context.Saga.UserLogin,
                        Data = JsonSerializer.Serialize(new { value = context.Saga.Amount })
                    })).Finalize()
            ),
        //обрабатываем ошибки подтверждения/отката транзакции - шлем уведомление пользователю
        When(FaultNotificationEvent)
            .Send(
            new Uri(configuration["QueuePaths:ErrorNotificationConsumer"]),
                context => new NotificationServiceError
                {
                    CorrelationId = context.Saga.CorrelationId,
                    ErrorMessage = context.Message.Message.ErrorMessage,
                    ErrorExceptionMessage = context.Message.Message.ErrorExceptionMessage,
                    ErrorServiceName = context.Message.Message.ErrorServiceName,
                    UserLogin = context.Saga.UserLogin,
                    TraceId = Guid.NewGuid(),
                    IsError = context.Saga.IsError
                }).Finalize()
        );
    }

}