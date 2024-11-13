using Common.Contracts;
using MassTransit;
using ProcessingService.Activities.Bid;

namespace ProcessingService.StateMachines.BidPlacedStateMachine;
public class BidPlacedStateMachine : MassTransitStateMachine<BidPlacedState>
{
    public State FinanceState { get; }
    public State BidState { get; }
    public State SearchState { get; }
    public State NotificationState { get; }
    public State ESCommitState { get; }
    public State CompletedState { get; }


    public Event<RequestBidPlace> RequestEvent { get; }
    public Event<BidFinanceGranted> FinanceEvent { get; }
    public Event<BidPlaced> BidEvent { get; }
    public Event<BidSearchPlaced> SearchEvent { get; }
    public Event<BidNotificationProcessed> NotificationEvent { get; }
    public Event<BidCreateESCommit> CommitEvent { get; }
    public Event<Fault<BidFinanceGranted>> FinanceFaultedEvent { get; }
    public Event<Fault<BidPlaced>> BidFaultedEvent { get; }
    public Event<Fault<BidSearchPlaced>> SearchFaultedEvent { get; }
    public Event<Fault<BidNotificationProcessed>> NotificationFaultedEvent { get; }

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
        ConfigureESCommitState();
        ConfigureCompleted();
    }

    private void ConfigureEvents()
    {
        Event(() => RequestEvent, p =>
        {
            //ВАЖНО! Для эвента в секции Initially - нужно указать этот флаг,
            //иначе первый раз после рестарта не срабатывает событие
            p.InsertOnInitial = true;
        });
        Event(() => FinanceEvent);
        Event(() => BidEvent);
        Event(() => SearchEvent);
        Event(() => NotificationEvent);
        Event(() => CommitEvent);
        Event(() => FinanceFaultedEvent, x => x.CorrelateById(
            context => context.Message.Message.CorrelationId));
        // Event(() => BidFinanceGrantedFaultedEvent, x => x.CorrelateById(
        //     context => context.Message.Message.CorrelationId));

        // Event(() => BidSearchPlacedFaultedEvent, x => x.CorrelateById(
        //     context => context.Message.Message.CorrelationId));
        // Event(() => BidNotificationFaultedEvent, x => x.CorrelateById(
        //     context => context.Message.Message.CorrelationId));
        // Event(() => ErrorBidEventSourcingCommitEvent);
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
            .TransitionTo(FinanceState)
            //делаем запись о списании денег, посылаем в лог в EventSourcing
            .Activity(p => p.OfType<FinanceActivity>())
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
            //делаем запись о создании ставки, посылаем в лог в EventSourcing
            .Activity(p => p.OfType<BidActivity>())
            .TransitionTo(BidState),
        When(FinanceFaultedEvent)
            //обработка ошибок пайплайна
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .TransitionTo(CompletedState)
            .Send(
                new Uri(configuration["QueuePaths:FaultedNotification"]),
                context => new FaultMessageSending(
                context.Saga.CorrelationId,
                context.Message.Exceptions[0].Message,
                context.Saga.Bidder
            ))

        );
    }

    private void ConfigureBidPState()
    {
        During(BidState,
        //успешно разместили ставку - публикуем обновленную величину ставки
        When(BidEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //успешно разместили запись о новой ставке в соответствующем аукционе - размещаем
            //запись о новой ставке в Search для публикации  
            .Activity(p => p.OfType<SearchActivity>())
            .TransitionTo(SearchState)
        );
    }



    private void ConfigureSearchState()
    {
        During(SearchState,
        //успешно опубликовали новую ставку - делаем оповещение
        When(SearchEvent)

            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Activity(p => p.OfType<NotificationActivity>())
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
            //посылаем запрос о подтверждении транзакции
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
            .Send(
                new Uri(configuration["QueuePaths:RollbackBidPlaced"]),
                context => new RollbackBidPlaced(
                context.Saga.BidId,
                context.Saga.CorrelationId
            ))
            .TransitionTo(CompletedState));
    }

    private void ConfigureCompleted()
    {
        During(CompletedState);
    }

}