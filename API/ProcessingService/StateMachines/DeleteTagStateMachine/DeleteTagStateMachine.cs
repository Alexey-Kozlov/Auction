using System.Text.Json;
using Common.Contracts.Processing;
using Common.Contracts.Tag;
using MassTransit;
using ProcessingService.Activities.TagDelete;

namespace ProcessingService.StateMachines.DeleteTagStateMachine;

public class DeleteTagStateMachine : MassTransitStateMachine<DeleteTagState>
{
    public State DeleteTagState { get; }
    public State PreCommitState { get; }
    public State CommitState { get; }
    public State TagListState { get; }
    public State CompleteState { get; }

    public Event<RequestDeleteTag> RequestDeleteEvent { get; }
    public Event<ESLogTagDeleted> EsLogEvent { get; }
    public Event<TagDeleteESCommit> CommitEvent { get; }
    public Event<TagListCommit> TagListEvent { get; }
    public Event<TagDeleteNotificationEvent> NotificationUIEvent { get; }
    public Event<Fault<ESLogTagDeleted>> FaultEsLogEvent { get; }
    public Event<Fault<TagDeleteESCommit>> FaultCommitEvent { get; }
    public Event<Fault<TagListCommit>> FaultTagListEvent { get; }
    public Event<BaseServiceError> FaultEvent { get; }
    public Event<Fault<TagDeleteNotificationEvent>> FaultNotificationUIEvent { get; }
    public object locker = new();
    private IConfiguration configuration { get; }

    public DeleteTagStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureTagState();
        ConfigurePreCommitState();
        ConfigureCommitState();
        ConfigureGetTagList();
        ConfigureCompletedState();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestDeleteEvent, p =>
        {
            p.InsertOnInitial = true;
        });
        Event(() => EsLogEvent);
        Event(() => CommitEvent);
        Event(() => NotificationUIEvent);
        Event(() => TagListEvent);
        Event(() => FaultEvent);
        Event(() => FaultEsLogEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultCommitEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultNotificationUIEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultTagListEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
    }
    private void ConfigureInitialState()
    {
        //поступил запрос на удаление тега для аукциона
        Initially(
            When(RequestDeleteEvent)
            .Then(context =>
            {
                context.Saga.Name = context.Message.Name;
                context.Saga.AuctionId = context.Message.AuctionId;
                context.Saga.ItemId = Guid.NewGuid();
                context.Saga.IsError = false;
                context.Saga.UserLogin = context.Message.UserLogin;
                context.Saga.CommitCounter = 2;
            })
            //посылаем через Кафку, выполнение всех операций в ES лог для записи тега:
            .Activity(p => p.OfType<ESLogActivity>()
            .TransitionTo(DeleteTagState))
        );
        SetCompletedWhenFinalized();
    }

    private void ConfigureTagState()
    {
        During(DeleteTagState,
        When(EsLogEvent)
        //посылаем сообщение на удаление тега в TagService
            .Send(
                new Uri(configuration["QueuePaths:TagDeleteConsumer"]),
                context => new ModifyTag
                {
                    AuctionId = context.Saga.AuctionId,
                    Name = context.Saga.Name,
                    ItemId = JsonSerializer.Deserialize<TagItem>(context.Message.DataItems.DataObjects[0].Data).ItemId,
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Tag.TagDeleteESCommit",
                    Commited = false
                })
            .TransitionTo(PreCommitState),
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

    private void ConfigurePreCommitState()
    {
        During(PreCommitState,
        When(CommitEvent)
            .Publish(context => new TagDeleteESCommit
            {
                CorrelationId = context.Saga.CorrelationId
            })
        .TransitionTo(CommitState),
        When(FaultCommitEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new TagDeleteESCommit
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
            .Publish(context => new TagDeleteESCommit
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.ErrorMessage,
                ErrorExceptionMessage = context.Message.ErrorExceptionMessage,
                ErrorServiceName = context.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
        .TransitionTo(CommitState));
    }

    private void ConfigureCommitState()
    {
        During(CommitState,
        When(CommitEvent)
            //подтверждаем/откатываем транзакцию
            .Activity(p => p.OfType<CommitActivity>())
            .TransitionTo(TagListState)
        );
    }

    private void ConfigureGetTagList()
    {
        During(TagListState,
        When(TagListEvent)
            //ждем сообщений о завершении транзакций в EventSourcing и Tag
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
                //ошибок нет
                p => p
                //Запрашиваем обновленный список тегов
                    .Publish(context => new TagListRequest
                    {
                        AuctionId = context.Message.AuctionId,
                        CorrelationId = context.Saga.CorrelationId,
                        CallBackType = "Common.Contracts.Tag.TagDeleteNotificationEvent",
                    })
                  ).TransitionTo(CompleteState)
              ),
        When(FaultTagListEvent)
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

    private void ConfigureCompletedState()
    {
        During(CompleteState,
        When(NotificationUIEvent)
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
            //Создаем событие в сервис NotificationService для обновления интерфейса -
            //передаем список уникальных тегов
            .Send(
                new Uri(configuration["QueuePaths:EventNotificationConsumer"]),
                context => new EventNotificationItem
                {
                    SignalRMethod = SignalRMethod.TagDeleted,
                    Show = true,
                    EventType = EventType.UserLogin,
                    UserLogin = context.Saga.UserLogin,
                    Data = context.Message.Data
                }).Finalize()
        ),
        When(FaultNotificationUIEvent)
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
            })
            .Finalize()
        );
    }

}