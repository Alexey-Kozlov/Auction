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
    public Event<Fault<FinanceCreateESCommit>> PreCommitEvent { get; }
    public Event<FinanceCreateESCommit> CommitEvent { get; }
    public Event<Fault<ESLogFinanceCreated>> FaultEsLogEvent { get; }
    public Event<Fault<FinanceNotificationCreated>> FaultNotificationEvent { get; }
    public Event<BaseServiceError> FaultEvent { get; }
    private IConfiguration configuration { get; }

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
        Event(() => PreCommitEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
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
                context.Saga.UserLogin = context.Message.UserLogin;
                context.Saga.SessionId = context.Message.SessionId;
                context.Saga.IsError = false;
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
                Message = context.Message.Message.Message,
                ExceptionMessage = context.Message.Message.ExceptionMessage,
                ServiceName = context.Message.Message.ServiceName,
                UserLogin = context.Saga.UserLogin,
                IsError = context.Message.Message.IsError
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
                IsError = context.Message.IsError,
            })
        .TransitionTo(CommitState),
        When(PreCommitEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Send(
                new Uri(configuration["QueuePaths:ErrorNotificationConsumer"]),
                context => new NotificationServiceError
                {
                    CorrelationId = context.Saga.CorrelationId,
                    Message = context.Message.Message.Message,
                    ExceptionMessage = context.Message.Message.ExceptionMessage,
                    ServiceName = context.Message.Message.ServiceName,
                    UserLogin = context.Saga.UserLogin,
                    IsError = context.Message.Message.IsError
                })
            .Publish(context => new FinanceCreateESCommit
            {
                CorrelationId = context.Saga.CorrelationId,
                IsError = context.Message.Message.IsError,
            })
        .TransitionTo(CommitState),
        //обработка ошибок - передаем отмену коммита и инфу по ошибке пользователю в UI
        When(FaultEvent)
            .Then(p => p.Saga.IsError = p.Message.IsError)
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
                    IsError = context.Message.IsError
                })
            .Publish(context => new FinanceCreateESCommit
            {
                CorrelationId = context.Saga.CorrelationId,
                IsError = context.Message.IsError,
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
            .Send(
                new Uri(configuration["QueuePaths:FinanceNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>
                    {
                        new DataForProcessingService
                        {
                            CRUD = CRUD.Create,
                            DataType = nameof(FinanceItem),
                            Data = JsonSerializer.Serialize(new NotifyItem
                            {
                                AuctionId = Guid.NewGuid(),
                                UserLogin = context.Saga.UserLogin
                            })
                        }
                    },
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Finance.FinanceCreateComplete",
                    Props = $"{context.Saga.Amount.ToString()},{!context.Saga.IsError}"
                })
            .Finalize(),
        //обрабатываем ошибки подтверждения/отката транзакции - шлем уведомление пользователю
        When(FaultNotificationEvent)
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
                    IsError = context.Message.Message.IsError
                })
            .Finalize()
        );
    }

}