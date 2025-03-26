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
    public State FaultCommitState { get; }
    public State CommitState { get; }
    public State CompletedState { get; }


    public Event<RequestBidPlace> RequestEvent { get; }
    public Event<ESLogPlaceBid> EsLogEvent { get; }
    public Event<BidPlaced> BidEvent { get; }
    public Event<BidSearchPlaced> SearchEvent { get; }
    public Event<BidNotificationProcessed> NotificationEvent { get; }
    public Event<BidCreateESCommit> CommitEvent { get; }
    public Event<BidComplete> CompleteEvent { get; }
    public Event<Fault<ESLogPlaceBid>> FaultEsLogEvent { get; }
    public Event<Fault<BidPlaced>> FaultBidEvent { get; }
    public Event<Fault<BidSearchPlaced>> FaultSearchEvent { get; }
    public Event<Fault<BidNotificationProcessed>> FaultNotificationEvent { get; }
    public Event<Fault<BidCreateESCommit>> FaultCommitEvent { get; }
    public Event<Fault<BidComplete>> FaultCompleteEvent { get; }
    private IConfiguration configuration { get; }


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
        ConfigureFaultCommitState();
        ConfigureCommitState();
        ConfigureCompletedState();
    }

    private void ConfigureEvents()
    {
        Event(() => RequestEvent, p =>
        {
            //ВАЖНО! Для эвента в секции Initially - нужно указать этот флаг,
            //иначе первый раз после рестарта не срабатывает событие
            p.InsertOnInitial = true;
        });
        Event(() => CompleteEvent);
        Event(() => BidEvent);
        Event(() => SearchEvent);
        Event(() => NotificationEvent);
        Event(() => CommitEvent);
        Event(() => EsLogEvent);
        Event(() => FaultEsLogEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultCommitEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultBidEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultNotificationEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultSearchEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultCompleteEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
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
            .Publish(contex => new BidCreateESCommit
            {
                CorrelationId = contex.Saga.CorrelationId,
                IsError = true
            })
        .TransitionTo(FaultCommitState)
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
            .Publish(contex => new BidCreateESCommit
            {
                CorrelationId = contex.Saga.CorrelationId,
                IsError = true
            })
        .TransitionTo(FaultCommitState)
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
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == nameof(AuctionItem)).ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Bid.BidNotificationProcessed"
                })
            .TransitionTo(NotificationState),
        //обрабатываем ошибки из сервиса BidService
        When(FaultSearchEvent)
            .Publish(contex => new BidCreateESCommit
            {
                CorrelationId = contex.Saga.CorrelationId,
                IsError = true
            })
        .TransitionTo(FaultCommitState)
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
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == nameof(NotifyItem)).ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Bid.BidCreateESCommit",
                    Props = context.Saga.Amount.ToString()
                })
            .TransitionTo(CommitState),
        //обрабатываем ошибки из сервиса NotificationService
        When(FaultNotificationEvent)
            .Publish(contex => new BidCreateESCommit
            {
                CorrelationId = contex.Saga.CorrelationId,
                IsError = true
            })
        .TransitionTo(FaultCommitState)
        );
    }

    private void ConfigureFaultCommitState()
    {
        During(FaultCommitState,
        //передаем ошибки пользователю
        When(FaultCommitEvent)
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
                IsError = context.Message.Message.IsError
            })
            .Publish(contex => new BidCreateESCommit
            {
                CorrelationId = contex.Saga.CorrelationId,
                IsError = true
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
            .TransitionTo(CompletedState)
        );
    }

    private void ConfigureCompletedState()
    {
        During(CompletedState,
        When(CompleteEvent)
        .Finalize(),
        //обрабатываем ошибки из сервиса EventSourcing - ESCommit
        When(FaultCompleteEvent)
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
                    IsError = context.Message.Message.IsError
                })
        .Finalize()
        );
    }

}