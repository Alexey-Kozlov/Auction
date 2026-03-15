using System.Text.Json;
using Common.Contracts.Communication;
using Common.Contracts.Processing;
using MassTransit;
using ProcessingService.Activities.CommunicationDelete;

namespace ProcessingService.StateMachines.DeleteCommunicationStateMachine;

public class DeleteCommunicationStateMachine : MassTransitStateMachine<DeleteCommunicationState>
{
    public State MessageState { get; }
    public State SearchState { get; }
    public State PreCommitState { get; }
    public State CommitState { get; }
    public State CompleteState { get; }
    public State AbortState { get; }


    public Event<RequestCommunicationDelete> RequestEvent { get; }
    public Event<ESLogCommunicationDeleted> EsLogEvent { get; }
    public Event<CommunicationDeleteSearch> SearchEvent { get; }
    public Event<CommunicationDeleteESCommit> CommitEvent { get; }
    public Event<CommunicationDeleteNotificationEvent> NotificationEvent { get; }
    public Event<BaseServiceError> FaultEvent { get; }
    public Event<Fault<ESLogCommunicationDeleted>> FaultEsLogEvent { get; }
    public Event<Fault<CommunicationDeleteSearch>> FaultSearchEvent { get; }
    public Event<Fault<CommunicationDeleteESCommit>> FaultCommitEvent { get; }
    public Event<Fault<CommunicationDeleteNotificationEvent>> FaultNotificationEvent { get; }
    public object locker = new();
    private IConfiguration configuration { get; }


    public DeleteCommunicationStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureMessageState();
        ConfigureSearchState();
        ConfigurePreCommitState();
        ConfigureCommitState();
        ConfigureCompleteState();
        ConfigureAbortState();
    }

    private void ConfigureEvents()
    {
        Event(() => RequestEvent, p =>
        {
            //ВАЖНО! Для эвента в секции Initially - нужно указать этот флаг,
            //иначе первый раз после рестарта не срабатывает событие
            p.InsertOnInitial = true;
        });
        Event(() => EsLogEvent);
        Event(() => SearchEvent);
        Event(() => CommitEvent);
        Event(() => NotificationEvent);
        Event(() => FaultEvent);
        Event(() => FaultEsLogEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultCommitEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultSearchEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultNotificationEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
    }
    private void ConfigureInitialState()
    {
        //поступил запрос на удаление сообщения чата
        Initially(
            When(RequestEvent)
            .Then(context =>
            {
                context.Saga.UserLogin = context.Message.UserLogin;
                context.Saga.ItemId = context.Message.ItemId;
                context.Saga.AuctionId = context.Message.AuctionId;
                context.Saga.IsError = false;
                context.Saga.CommitCounter = 3;
            })
            .Activity(p => p.OfType<ESLogActivity>()
            .TransitionTo(MessageState))
        );
        SetCompletedWhenFinalized();
    }

    private void ConfigureMessageState()
    {
        During(MessageState,
        When(EsLogEvent)
            .Then(context =>
            {
                context.Saga.DataForProcessingServicesList = JsonSerializer.Serialize(context.Message.DataItems);
            })
        //отправляем сообщение на удаление чата в CommunicationService
            .Send(
            new Uri(configuration["QueuePaths:CommunicationsConsumer"]),
            context => new DataForProcessingServicesList<CommunicationItem>
            {
                DataObjects = context.Message.DataItems.DataObjects.ToList(),
                CorrelationId = context.Saga.CorrelationId,
                CallBackType = "Common.Contracts.Communication.CommunicationDeleteSearch"
            })
        .TransitionTo(SearchState),
        //обрабатываем ошибки из сервиса EventSourcingService            
        When(FaultEsLogEvent)
            .Then(p => p.Saga.IsError = true)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionMessage = context.Message.Message.ErrorExceptionMessage,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
            .TransitionTo(AbortState)
        );
    }

    private void ConfigureSearchState()
    {
        During(SearchState,
        When(SearchEvent)
        //отправляем сообщение для обновления поиска
            .Send(
                new Uri(configuration["QueuePaths:ElkCommunicationConsumer"]),
                context => new DataForProcessingServicesList<CommunicationItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList)
                        .DataObjects,
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Communication.CommunicationDeleteESCommit"
                })
            .TransitionTo(PreCommitState),
        When(FaultSearchEvent)
            .Then(p => p.Saga.IsError = true)
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
    на выходе - событие для подтверждения/отката транзакции
    */

    private void ConfigurePreCommitState()
    {
        During(PreCommitState,
        When(CommitEvent)
            .Publish(context => new CommunicationDeleteESCommit
            {
                CorrelationId = context.Saga.CorrelationId
            })
        .TransitionTo(CommitState),
        When(FaultCommitEvent)
            .Then(p => p.Saga.IsError = true)
            .Publish(context => new CommunicationDeleteESCommit
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
            .Publish(context => new CommunicationDeleteESCommit
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
            //подтверждаем/откатываем транзакцию
            .Activity(p => p.OfType<CommitActivity>())
            .TransitionTo(CompleteState)
        );
    }

    private void ConfigureCompleteState()
    {
        During(CompleteState,
        When(NotificationEvent)
            //ждем сообщений об обработке всех 3 коллекций с записями
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
                    }).Finalize(),
                p => p
                //Создаем событие в сервис NotificationService для обновления интерфейса
                .Send(
                    new Uri(configuration["QueuePaths:EventNotificationConsumer"]),
                    context => new EventNotificationItem
                    {
                        SignalRMethod = SignalRMethod.CommunicationDelete,
                        AuctionId = context.Saga.AuctionId,
                        ItemId = context.Saga.ItemId,
                        Show = !context.Saga.IsError,
                        EventType = EventType.Page,
                        Data = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList)
                            .DataObjects[0].Data,
                        UserLogin = context.Saga.UserLogin,
                        //Page - URL страницы вида "communication/<id аукциона>"
                        Page = "communication/" + context.Saga.AuctionId.ToString()
                    })).Finalize()
            ),
        //обрабатываем ошибки подтверждения/отката транзакции            
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
                ItemId = context.Saga.ItemId,
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
                ErrorExceptionMessage = context.Message.ErrorExceptionMessage,
                ErrorServiceName = context.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin,
                TraceId = Guid.NewGuid(),
            })
        .Finalize());
    }
}