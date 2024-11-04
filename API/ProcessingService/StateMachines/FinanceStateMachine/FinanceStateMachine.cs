using Common.Contracts;
using MassTransit;
using ProcessingService.Activities.Finance;

namespace ProcessingService.StateMachines.FinanceStateMachine;
public class FinanceStateMachine : MassTransitStateMachine<FinanceState>
{
    public State FinanceState { get; }
    public State NotificationState { get; }
    public State ESCommitState { get; }
    public State CompletedState { get; }


    public Event<RequestCreateFinance> RequestEvent { get; }
    public Event<FinanceCreated> FinanceEvent { get; }
    public Event<FinanceNotificationCreated> NotificationEvent { get; }
    public Event<FinanceCreateESCommit> CommitEvent { get; }
    private IConfiguration configuration { get; }

    public FinanceStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureFinanceState();
        ConfigureNotificationState();
        ConfigureESCommitState();
        ConfigureCompleted();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestEvent, p =>
        {
            p.InsertOnInitial = true;
        });
        Event(() => FinanceEvent);
        Event(() => NotificationEvent);
        Event(() => CommitEvent);
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
            //посылаем через Кафку в EventSourcingService -> CreateEventSourcingItemConsumer
            //Создание записи по добавлению денег в сервисе FinanceService
            .Activity(p => p.OfType<FinanceActivity>())
            .TransitionTo(FinanceState)
        );
    }

    private void ConfigureFinanceState()
    {
        During(FinanceState,
        //создали записи в ES и FinanceService о поступлении денег
        When(FinanceEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //делаем рассылку о поступлении денег, сервис NotificationService
            .Send(
                new Uri(configuration["QueuePaths:FinanceCreatingNotification"]),
                context => new FinanceCreatingNotification(
                context.Saga.Amount,
                context.Saga.UserLogin,
                context.Saga.CorrelationId
                ))
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
            //посылаем через Кафку в EventSourcingService - для подтверждения транзакции
            .Activity(p => p.OfType<CommitActivity>())
            .TransitionTo(ESCommitState));
    }

    private void ConfigureESCommitState()
    {
        During(ESCommitState,
        When(CommitEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .TransitionTo(CompletedState));
    }

    private void ConfigureCompleted()
    {
        During(CompletedState);
    }

}