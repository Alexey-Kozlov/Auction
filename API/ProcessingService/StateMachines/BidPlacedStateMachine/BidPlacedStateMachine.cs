using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Bid;
using Common.Contracts.Finance;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using MassTransit;
using ProcessingService.Activities.Bid;

namespace ProcessingService.StateMachines.BidPlacedStateMachine;
public class BidPlacedStateMachine : MassTransitStateMachine<BidPlacedState>
{
    public State FinanceState { get; }
    public State BidState { get; }
    public State SearchState { get; }
    public State NotificationState { get; }
    public State PreCommitState { get; }
    public State CommitState { get; }
    public State CompleteState { get; }


    public Event<RequestBidPlace> RequestEvent { get; }
    public Event<ESLogPlaceBid> EsLogEvent { get; }
    public Event<BidPlaced> BidEvent { get; }
    public Event<BidSearchPlaced> SearchEvent { get; }
    public Event<BidNotification> NotificationEvent { get; }
    public Event<BidNotificationEvent> NotificationUIEvent { get; }
    public Event<BidCreateESCommit> CommitEvent { get; }
    public Event<BaseServiceError> FaultEvent { get; }
    public Event<Fault<ESLogPlaceBid>> FaultEsLogEvent { get; }
    public Event<Fault<BidPlaced>> FaultBidEvent { get; }
    public Event<Fault<BidSearchPlaced>> FaultSearchEvent { get; }
    public Event<Fault<BidNotification>> FaultNotificationEvent { get; }
    public Event<Fault<BidNotificationEvent>> FaultNotificationUIEvent { get; }
    public Event<Fault<BidCreateESCommit>> FaultCommitEvent { get; }
    private IConfiguration configuration { get; }
    public object locker = new();


    public BidPlacedStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureFinanceState();
        ConfigureBidPState();
        ConfigureSearchState();
        ConfigureNotificationState();
        ConfigurePreCommitState();
        ConfigureCommitState();
        ConfigureCompleteState();
    }

    private void ConfigureEvents()
    {
        Event(() => RequestEvent, p =>
        {
            //ВАЖНО! Для эвента в секции Initially - нужно указать этот флаг,
            //иначе первый раз после рестарта не срабатывает событие
            p.InsertOnInitial = true;
        });
        Event(() => BidEvent);
        Event(() => SearchEvent);
        Event(() => NotificationEvent);
        Event(() => CommitEvent);
        Event(() => EsLogEvent);
        Event(() => FaultEvent);
        Event(() => FaultEsLogEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultCommitEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultBidEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultNotificationEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultSearchEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultNotificationUIEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
    }
    private void ConfigureInitialState()
    {
        //поступил запрос на создание ставки
        Initially(
            When(RequestEvent)
            .Then(context =>
            {
                context.Saga.Bidder = context.Message.Bidder;
                context.Saga.AuctionId = context.Message.AuctionId;
                context.Saga.Amount = context.Message.Amount;
                context.Saga.BidId = Guid.NewGuid();
                context.Saga.IsError = false;
                context.Saga.CommitCounter = 5;
            })
            //посылаем через Кафку, выполнение всех операций в ES лог для создания ставки:
            // - Возврат денег по предыдущей ставке (если была) - возврат денег и баланса предыдущего пользователя в FinanceService
            // - Проверка на превышение ставкой текущего баланса
            // - Создание записи в ES лог по списанию денег, списание денег и возврат баланса текущего пользователя в FinanceService
            .Activity(p => p.OfType<ESLogActivity>()
            .TransitionTo(FinanceState))
        );
        SetCompletedWhenFinalized();
    }

    private void ConfigureFinanceState()
    {
        During(FinanceState,
        When(EsLogEvent)
            .Then(context =>
            {
                context.Saga.DataForProcessingServicesList = JsonSerializer.Serialize(context.Message.DataItems);
            })
            .Send(
            new Uri(configuration["QueuePaths:FinanceConsumer"]),
            context => new DataForProcessingServicesList<FinanceItem>
            {
                DataObjects = context.Message.DataItems.DataObjects.Where(p => p.DataType == nameof(FinanceItem)).ToList(),
                CorrelationId = context.Saga.CorrelationId,
                CallBackType = "Common.Contracts.Bid.BidPlaced"
            })
        .TransitionTo(BidState),
        //обрабатываем ошибки из сервиса EventSourcingService            
        When(FaultEsLogEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                Message = context.Message.Message.Message,
                ExceptionMessage = context.Message.Message.ExceptionMessage,
                ServiceName = context.Message.Message.ServiceName,
                UserLogin = context.Saga.Bidder
            })
            .TransitionTo(PreCommitState)
        );
    }

    private void ConfigureBidPState()
    {
        During(BidState,
        When(BidEvent)
            .Send(
                new Uri(configuration["QueuePaths:BidConsumer"]),
                context => new DataForProcessingServicesList<BidItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == nameof(BidItem)).ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Bid.BidSearchPlaced"
                })
            .TransitionTo(SearchState),
        //обрабатываем ошибки из сервиса FinanceService            
        When(FaultBidEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                Message = context.Message.Message.Message,
                ExceptionMessage = context.Message.Message.ExceptionMessage,
                ServiceName = context.Message.Message.ServiceName,
                UserLogin = context.Saga.Bidder
            })
            .TransitionTo(PreCommitState)
        );
    }

    private void ConfigureSearchState()
    {
        During(SearchState,
        //публикуем обновленную ставку в SearchService
        When(SearchEvent)
            .Send(
                new Uri(configuration["QueuePaths:SearchConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p =>
                        p.DataType == nameof(AuctionItem)).ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Bid.BidNotification"
                })
            .TransitionTo(NotificationState),
        //обрабатываем ошибки из сервиса BidService
        When(FaultSearchEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                Message = context.Message.Message.Message,
                ExceptionMessage = context.Message.Message.ExceptionMessage,
                ServiceName = context.Message.Message.ServiceName,
                UserLogin = context.Saga.Bidder
            })
            .TransitionTo(PreCommitState)
        );
    }

    private void ConfigureNotificationState()
    {
        During(NotificationState,
        //успешно опубликовали новую ставку - делаем оповещение
        When(NotificationEvent)
            .Send(
                new Uri(configuration["QueuePaths:BidNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p =>
                        p.DataType == nameof(NotifyItem)).ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Bid.BidCreateESCommit",
                    Props = ""
                })
            .TransitionTo(PreCommitState),
        //обрабатываем ошибки из сервиса NotificationService
        When(FaultNotificationEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(contex => new BidCreateESCommit
            {
                CorrelationId = contex.Saga.CorrelationId
            })
        .TransitionTo(PreCommitState)
        );
    }

    /*промежуточный этап перед подтверждением/откатом транзакции
    на входе события:
    - BaseServiceError - событие ошибок от предыдущих этапов
    - Fault<BidCreateESCommit> - событие ошибки предыдущего этапа
    - BidCreateESCommit - событие правильного выполнения предыдущего этапа
    на выходе - событие для подтверждения/отката транзакции - BidCreateESCommit
    */

    private void ConfigurePreCommitState()
    {
        During(PreCommitState,
        When(CommitEvent)
            .Publish(context => new BidCreateESCommit
            {
                CorrelationId = context.Saga.CorrelationId
            })
        .TransitionTo(CommitState),
        When(FaultCommitEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Send(
                new Uri(configuration["QueuePaths:ErrorNotificationConsumer"]),
                context => new NotificationServiceError
                {
                    CorrelationId = context.Saga.CorrelationId,
                    Message = context.Message.Message.Message,
                    ExceptionMessage = context.Message.Message.ExceptionMessage,
                    ServiceName = context.Message.Message.ServiceName,
                    UserLogin = context.Saga.Bidder,
                    IsError = context.Message.Message.IsError
                })
            .Publish(context => new BidCreateESCommit
            {
                CorrelationId = context.Saga.CorrelationId
            })
        .TransitionTo(CommitState),
        //обработка ошибок - передаем отмену коммита и инфу по ошибке пользователю в UI
        When(FaultEvent)
            .Send(
                new Uri(configuration["QueuePaths:ErrorNotificationConsumer"]),
                context => new NotificationServiceError
                {
                    CorrelationId = context.Saga.CorrelationId,
                    Message = context.Message.Message,
                    ExceptionMessage = context.Message.ExceptionMessage,
                    ServiceName = context.Message.ServiceName,
                    UserLogin = context.Saga.Bidder,
                    TraceId = Guid.NewGuid(),
                    IsError = context.Saga.IsError
                })
            .Publish(context => new BidCreateESCommit
            {
                CorrelationId = context.Saga.CorrelationId
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
        When(NotificationUIEvent)
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
                .IfElse(context => context.Saga.DataForProcessingServicesList == null,
                p => p
                //Создаем событие в сервис NotificationService для обновления интерфейса
                    .Send(
                    new Uri(configuration["QueuePaths:BidEventNotificationConsumer"]),
                    context => new DataForProcessingServicesList<NotifyItem>
                    {
                        DataObjects = new List<DataForProcessingService>(),
                        CorrelationId = context.Saga.CorrelationId,
                        CallBackType = "",
                        Props = $"{!context.Saga.IsError}"
                    }),
                p => p
                //Создаем событие в сервис NotificationService для обновления интерфейса
                .Send(
                    new Uri(configuration["QueuePaths:BidEventNotificationConsumer"]),
                    context => new DataForProcessingServicesList<NotifyItem>
                    {
                        DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p =>
                            p.DataType == nameof(NotifyItem)).ToList(),
                        CorrelationId = context.Saga.CorrelationId,
                        CallBackType = "",
                        Props = $"{!context.Saga.IsError}"
                    })).Finalize()
            ),
        //обрабатываем ошибки подтверждения/отката транзакции            
        When(FaultNotificationUIEvent)
        .Send(
            new Uri(configuration["QueuePaths:ErrorNotificationConsumer"]),
            context => new NotificationServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                Message = context.Message.Message.Message,
                ExceptionMessage = context.Message.Message.ExceptionMessage,
                ServiceName = context.Message.Message.ServiceName,
                UserLogin = context.Saga.Bidder,
                TraceId = Guid.NewGuid(),
                AuctionId = context.Saga.AuctionId,
                IsError = context.Saga.IsError
            })
            .Finalize()
        );
    }
}