using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Processing;
using MassTransit;
using ProcessingService.Activities.AuctionFinish;

namespace ProcessingService.StateMachines.FinishAuctionStateMachine;
public class FinishAuctionStateMachine : MassTransitStateMachine<FinishAuctionState>
{
    public State ElkState { get; }
    public State NotificationState { get; }
    public State PreCommitState { get; }
    public State CommitState { get; }
    public State CompleteState { get; }

    public Event<ESLogAuctionFinish> RequestEvent { get; }
    public Event<AuctionFinishedElk> ElkEvent { get; }
    public Event<AuctionFinishedCommit> CommitEvent { get; }
    public Event<AuctionFinishedNotification> NotificationEvent { get; }
    public Event<BaseServiceError> FaultEvent { get; }
    public Event<Fault<ESLogAuctionFinish>> FaultRequestEvent { get; }
    public Event<Fault<AuctionFinishedElk>> FaultElkEvent { get; }
    public Event<Fault<AuctionFinishedCommit>> FaultCommitEvent { get; }
    public Event<Fault<AuctionFinishedNotification>> FaultNotificationEvent { get; }
    private IConfiguration configuration { get; }
    public object locker = new();


    public FinishAuctionStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureELKState();
        ConfigurePreCommitState();
        ConfigureCommitState();
        ConfigureCompleteState();

    }
    private void ConfigureEvents()
    {
        Event(() => RequestEvent, p => p.InsertOnInitial = true);
        Event(() => ElkEvent);
        Event(() => CommitEvent);
        Event(() => NotificationEvent);
        Event(() => FaultEvent);
        Event(() => FaultRequestEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultElkEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultCommitEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultNotificationEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
    }
    private void ConfigureInitialState()
    {
        //В сервисе EventSourcingService периодически срабатывает сервис CheckAuctionFinished, если найдены завершенные
        //аукционы - они завершаются и передается список аукционов для обновения в AuctionService
        Initially(
            When(RequestEvent)
                .Then(context =>
                {
                    context.Saga.Amount = context.Message.DataItems.DataObjects.Count();
                    context.Saga.DataForProcessingServicesList = JsonSerializer.Serialize(context.Message.DataItems);
                    context.Saga.IsError = false;
                    context.Saga.CommitCounter = 3;
                })
                //Обновление аукциона в сервисе SearchService
                .Send(
                    new Uri(configuration["QueuePaths:SearchConsumer"]),
                    context => new DataForProcessingServicesList<AuctionItem>
                    {
                        DataObjects = context.Message.DataItems.DataObjects,
                        CorrelationId = context.Saga.CorrelationId,
                        CallBackType = "Common.Contracts.Auction.AuctionFinishedElk"
                    })
                .TransitionTo(ElkState),
            When(FaultRequestEvent)
                .Then(p =>
                {
                    p.Saga.IsError = true;
                    p.Saga.CommitCounter = 3;
                })
                .Publish(context => new BaseServiceError
                {
                    CorrelationId = context.Saga.CorrelationId,
                    ErrorMessage = context.Message.Message.ErrorMessage,
                    ErrorExceptionMessage = context.Message.Message.ErrorExceptionMessage,
                    ErrorServiceName = context.Message.Message.ErrorServiceName,
                    UserLogin = "SystemService"
                })
                .TransitionTo(PreCommitState)
        );
        SetCompletedWhenFinalized();
    }

    private void ConfigureELKState()
    {
        During(ElkState,
        When(ElkEvent)
            //Обновление аукциона в поиске в сервисе ElasticSearchService
            .Send(
                new Uri(configuration["QueuePaths:ElkConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects,
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionFinishedCommit"
                })
            .TransitionTo(PreCommitState),
        //обрабатываем ошибки из EventSourcing при завершении аукционов            
        When(FaultElkEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionMessage = context.Message.Message.ErrorExceptionMessage,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = "SystemService"
            })
            .TransitionTo(PreCommitState)
        );
    }

    /*промежуточный этап перед подтверждением/откатом транзакции
    на входе события:
    - BaseServiceError - событие ошибок от предыдущих этапов
    - Fault<AuctionCreateESCommit> - событие ошибки предыдущего этапа
    - AuctionCreateESCommit - событие правильного выполнения предыдущего этапа
    на выходе - событие для подтверждения/отката транзакции - AuctionCreateESCommit
    */

    private void ConfigurePreCommitState()
    {
        During(PreCommitState,
        When(CommitEvent)
            .Publish(context => new AuctionFinishedCommit
            {
                CorrelationId = context.Saga.CorrelationId
            })
        .TransitionTo(CommitState),
        When(FaultCommitEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new AuctionFinishedCommit
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
            .Publish(context => new AuctionFinishedCommit
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.ErrorMessage,
                ErrorExceptionMessage = context.Message.ErrorExceptionMessage,
                ErrorServiceName = context.Message.ErrorServiceName,
                UserLogin = "SystemService"
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
            .TransitionTo(CompleteState));
    }

    private void ConfigureCompleteState()
    {
        During(CompleteState,
        When(NotificationEvent)
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
                        UserLogin = "SystemService",
                        TraceId = Guid.NewGuid(),
                        IsError = context.Saga.IsError
                    }).Finalize(),
                p => p
                    .Send(
                        new Uri(configuration["QueuePaths:AuctionFinishedNotificationConsumer"]),
                        context => new DataForProcessingServicesList<AuctionItem>
                        {
                            DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects,
                            CorrelationId = context.Saga.CorrelationId,
                            CallBackType = ""
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
                UserLogin = "SystemService",
                TraceId = Guid.NewGuid(),
                IsError = context.Saga.IsError
            })
        .Finalize()
        );
    }
}