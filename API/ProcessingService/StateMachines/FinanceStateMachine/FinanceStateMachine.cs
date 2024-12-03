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
    public State NotificationState { get; }
    public State CommitState { get; }
    public State CompletedState { get; }


    public Event<RequestCreateFinance> RequestEvent { get; }
    public Event<ESLog_FinanceCreated> EsLogEvent { get; }
    public Event<FinanceCreated> FinanceEvent { get; }
    public Event<FinanceNotificationCreated> NotificationEvent { get; }
    public Event<FinanceCreateESCommit> CommitEvent { get; }
    public Event<FinanceCreateComplete> CompleteEvent { get; }
    private IConfiguration configuration { get; }

    public FinanceStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureFinanceState();
        ConfigureNotificationState();
        ConfigureCommitState();
        ConfigureCompleted();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestEvent, p =>
        {
            p.InsertOnInitial = true;
        });
        Event(() => EsLogEvent);
        Event(() => FinanceEvent);
        Event(() => NotificationEvent);
        Event(() => CommitEvent);
        Event(() => CompleteEvent);
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestEvent)
            .Then(context =>
            {
                context.Saga.Amount = context.Message.Amount;
                context.Saga.UserLogin = context.Message.UserLogin;
                context.Saga.CorrelationId = context.Message.CorrelationId;
                context.Saga.SessionId = context.Message.SessionId;
                context.Saga.LastUpdated = DateTime.UtcNow;
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
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
                //делаем рассылку для создания новой записи поступления денег и корректировки баланса в FinanceService
                .Send(
                new Uri(configuration["QueuePaths:FinanceConsumer"]),
                context => new DataForProcessingServicesList<FinanceItem>
                {
                    DataObjects = context.Message.DataItems.DataObjects,
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Finance.FinanceNotificationCreated"
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
                    CallBackType = "Common.Contracts.Finance.FinanceCreateESCommit",
                    Props = context.Saga.Amount.ToString()
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
            //посылаем через Кафку в EventSourcingService - для подтверждения транзакции
            .Activity(p => p.OfType<CommitActivity>())
            .TransitionTo(CompletedState));
    }

    private void ConfigureCompleted()
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