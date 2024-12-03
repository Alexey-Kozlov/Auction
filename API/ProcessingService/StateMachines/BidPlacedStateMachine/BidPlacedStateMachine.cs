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
    public State CommitState { get; }
    public State CompletedState { get; }


    public Event<RequestBidPlace> RequestEvent { get; }
    public Event<ESLog_PlaceBid> EsLogEvent { get; }
    public Event<BidPlaced> BidEvent { get; }
    public Event<BidSearchPlaced> SearchEvent { get; }
    public Event<BidNotificationProcessed> NotificationEvent { get; }
    public Event<BidCreateESCommit> CommitEvent { get; }
    public Event<BidComplete> CompleteEvent { get; }
    public Event<Fault<ESLog_PlaceBid>> FaultEsLogEvent { get; }

    private IConfiguration configuration { get; }
    private DataForProcessingServicesList ListItems { get; set; }


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
        Event(() => FaultEsLogEvent, x => x.CorrelateById(
            context => context.Message.Message.CorrelationId));
    }
    private void ConfigureInitialState()
    {
        //поступил запрос на создание ставки
        Initially(
            When(RequestEvent)
            .Then(context =>
            {
                context.Saga.Bidder = context.Message.Bidder;
                context.Saga.LastUpdated = DateTime.UtcNow;
                context.Saga.AuctionId = context.Message.AuctionId;
                context.Saga.Amount = context.Message.Amount;
                context.Saga.CorrelationId = context.Message.CorrelationId;
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
                context.Saga.LastUpdated = DateTime.UtcNow;
                ListItems = context.Message.DataItems;
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
        When(FaultEsLogEvent)
        .Then(context =>
        {
            context.Saga.LastUpdated = DateTime.UtcNow;
        })
        .Send(
            new Uri(configuration["QueuePaths:ErrorNotificationConsumer"]),
            context => new DataForProcessingServicesList<NotifyItem>
            {
                DataObjects = new List<DataForProcessingService>
                {
                    new DataForProcessingService
                    {
                        DataType = "NotifyItem",
                        CRUD = CRUD.Read,
                        Data = JsonSerializer.Serialize(new NotifyItem
                        {
                            AuctionId = context.Saga.AuctionId,
                            UserLogin = context.Saga.Bidder
                        })
                    }
                },
                CorrelationId = context.Saga.CorrelationId,
                CallBackType = "Common.Contracts.Bid.BidComplete",
                Props = context.Message.Exceptions[0].Message
            })
        .TransitionTo(CompletedState)
        );
    }

    private void ConfigureBidPState()
    {
        During(BidState,
        When(BidEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:BidConsumer"]),
                context => new DataForProcessingServicesList<BidItem>
                {
                    DataObjects = ListItems.DataObjects.Where(p => p.DataType == nameof(BidItem)).ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Bid.BidSearchPlaced"
                })
            .TransitionTo(SearchState)
        );
    }

    private void ConfigureSearchState()
    {
        During(SearchState,
        //публикуем обновленную ставку в SearchService
        When(SearchEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:SearchConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = ListItems.DataObjects.Where(p => p.DataType == nameof(AuctionItem)).ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Bid.BidNotificationProcessed"
                })
            .TransitionTo(NotificationState));
    }

    private void ConfigureNotificationState()
    {
        During(NotificationState,
        //успешно опубликовали новую ставку - делаем оповещение
        When(NotificationEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:BidNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = ListItems.DataObjects.Where(p => p.DataType == nameof(NotifyItem)).ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Bid.BidCreateESCommit",
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

    private void ConfigureCompletedState()
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