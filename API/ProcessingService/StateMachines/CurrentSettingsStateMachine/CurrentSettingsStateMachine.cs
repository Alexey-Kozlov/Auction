using Common.Contracts.Processing;
using Common.Contracts.Settings;
using MassTransit;

namespace ProcessingService.StateMachines.CurrentSettingsStateMachine;

public class CurrentSettingsStateMachine : MassTransitStateMachine<CurrentSettingsState>
{
    public State SetCurrentSettingsState { get; }
    public State CompleteState { get; }


    public Event<RequestSetCurrentSettings> RequestSetCurrentSettingsEvent { get; }
    public Event<SetCurrentSettingsCompleted> CompleteEvent { get; }
    public Event<Fault<SetCurrentSettingsCompleted>> FaultCompleteEvent { get; }


    private IConfiguration configuration { get; }

    public CurrentSettingsStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureCompletedState();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestSetCurrentSettingsEvent, p =>
        {
            p.InsertOnInitial = true;
        });
        Event(() => CompleteEvent);
        Event(() => FaultCompleteEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestSetCurrentSettingsEvent)
            .Then(context =>
            {
                context.Saga.UserLogin = context.Message.UserLogin;
                context.Saga.AdminMode = context.Message.AdminMode;
                context.Saga.IsError = false;
                context.Saga.ShowMessages = true;
            })
            //посылаем сообщение для записи настроек системы в сервис Settings
            .Send(
                new Uri(configuration["QueuePaths:SetCurrentSettingsConsumer"]),
                context => new SetCurrentSettings
                {
                    CorrelationId = context.Saga.CorrelationId,
                    UserLogin = context.Message.UserLogin,
                    AdminMode = context.Saga.AdminMode,
                    CallBackType = "Common.Contracts.Settings.SetCurrentSettingsCompleted"
                })
            .TransitionTo(CompleteState)
        );
        SetCompletedWhenFinalized();
    }

    private void ConfigureCompletedState()
    {
        During(CompleteState,
        When(CompleteEvent)
            //Создаем событие в сервис NotificationService для обновления интерфейса
            .Send(
                new Uri(configuration["QueuePaths:EventNotificationConsumer"]),
                    context => new EventNotificationItem
                    {
                        SignalRMethod = SignalRMethod.SetCurrentSettings,
                        Show = context.Saga.ShowMessages,
                        EventType = EventType.UserLogin,
                        UserLogin = context.Saga.UserLogin,
                        Data = context.Saga.AdminMode ?
                                    $"Выполнен переход в административный режим"
                                    : "Выполнен переход в обычный режим"
                    })
            //обновляем состояние AdminMode в остальных сервисах
            .Publish(context => new SetAdminMode
            {
                CorrelationId = context.Saga.CorrelationId,
                AdminMode = context.Saga.AdminMode
            })
            .Finalize(),
        When(FaultCompleteEvent)
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
                        AuctionId = null,
                        IsError = context.Saga.IsError
                    })
                    .Finalize()
                );
    }

}