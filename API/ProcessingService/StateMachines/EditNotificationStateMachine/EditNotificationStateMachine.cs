using Common.Contracts.Notification;
using Common.Contracts.Processing;
using MassTransit;
using ProcessingService.Activities.EditNotification;

namespace ProcessingService.StateMachines.EditNotificationStateMachine;
public class EditNotificationStateMachine : MassTransitStateMachine<EditNotificationState>
{
    public State NotificationState { get; }
    public State CommitState { get; }
    public State CompletedState { get; }


    public Event<RequestEditNotification> RequestEvent { get; }
    public Event<ESLog_EditNotification> EsLogEvent { get; }
    public Event<EditNotificationESCommit> CommitEvent { get; }
    public Event<EditNotificationComplete> CompleteEvent { get; }
    private IConfiguration configuration { get; }

    public EditNotificationStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
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
        Event(() => CommitEvent);
        Event(() => CompleteEvent);
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestEvent)
            .Then(context =>
            {
                context.Saga.AuctionId = context.Message.AuctionId;
                context.Saga.Enable = context.Message.Enable;
                context.Saga.UserLogin = context.Message.UserLogin;
                context.Saga.CorrelationId = context.Message.CorrelationId;
                context.Saga.SessionId = context.Message.SessionId;
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
            .Send(
                new Uri(configuration["QueuePaths:EditNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = context.Message.DataItems.DataObjects,
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Notification.EditNotificationESCommit",
                    Props = context.Saga.UserLogin
                })
            .TransitionTo(CommitState));
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
        When(CompleteEvent).Finalize()
        );
    }

}