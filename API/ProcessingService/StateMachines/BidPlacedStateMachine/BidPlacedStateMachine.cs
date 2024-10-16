using Common.Contracts;
using MassTransit;
using ProcessingService.Activities.BidPlaced;

namespace ProcessingService.StateMachines.BidPlacedStateMachine;
public class BidPlacedStateMachine : MassTransitStateMachine<BidPlacedState>
{
    public State AfterEventSourcingState { get; }
    public State FinanceGrantedState { get; }
    public State GetCurrentBidState { get; }
    public State BidPlacedState { get; }
    public State BidSearchPlacedState { get; }
    public State UserNotificationSetState { get; }
    public State CommitBidPlacedState { get; }
    public State CompletedState { get; }
    public State FaultedState { get; }

    public Event<AfterBidPlacedContract> AfterBidPlacedEvent { get; }
    public Event<RequestBidPlace> RequestBidPlaceEvent { get; }
    public Event<BidFinanceGranted> BidFinanceGrantedEvent { get; }
    public Event<GetCurrentBid> GetCurrentBidEvent { get; }
    public Event<BidPlaced> BidPlacedEvent { get; }
    public Event<BidSearchPlaced> BidSearchPlacedEvent { get; }
    public Event<BidNotificationProcessed> BidNotificationProcessedEvent { get; }
    public Event<CommitBidPlacedContract> CommitBidPlacedEvent { get; }
    public Event<GetBidPlaceState> GetBidPlaceStateEvent { get; }
    public Event<Fault<BidFinanceGranting>> BidFinanceGrantedFaultedEvent { get; }
    public Event<Fault<BidPlacing>> BidPlacedFaultedEvent { get; }
    public Event<Fault<BidSearchPlacing>> BidSearchPlacedFaultedEvent { get; }
    private IConfiguration configuration { get; }

    public BidPlacedStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureAfterBidPlaced();
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureFinanceGranted();
        ConfigureBidAuctionPlaced();
        ConfigureBidPlaced();
        ConfigureBidSearchPlaced();
        ConfigureBidNotificationProcessed();
        ConfigureCommitBidPlaced();
        ConfigureCompleted();
        ConfigureAny();
        ConfigureFaulted();

    }

    private void ConfigureEvents()
    {
        Event(() => RequestBidPlaceEvent);
        Event(() => AfterBidPlacedEvent);
        Event(() => BidFinanceGrantedEvent);
        Event(() => GetCurrentBidEvent);
        Event(() => BidPlacedEvent);
        Event(() => BidSearchPlacedEvent);
        Event(() => BidNotificationProcessedEvent);
        Event(() => GetBidPlaceStateEvent);
        Event(() => CommitBidPlacedEvent);
        Event(() => BidFinanceGrantedFaultedEvent, x => x.CorrelateById(
            context => context.Message.Message.CorrelationId));
        Event(() => BidPlacedFaultedEvent, x => x.CorrelateById(
            context => context.Message.Message.CorrelationId));
        Event(() => BidSearchPlacedFaultedEvent, x => x.CorrelateById(
            context => context.Message.Message.CorrelationId));
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestBidPlaceEvent)
            .Then(context =>
            {
                context.Saga.Bidder = context.Message.Bidder;
                context.Saga.LastUpdated = DateTime.UtcNow;
                context.Saga.Id = context.Message.Id;
                context.Saga.Amount = context.Message.Amount;
                context.Saga.CorrelationId = context.Message.CorrelationId;
                context.Saga.BidId = Guid.NewGuid();
            })
            .Activity(p => p.OfType<BidPlacedActivity>())
            .TransitionTo(AfterEventSourcingState)
        );
    }

    private void ConfigureAfterBidPlaced()
    {
        During(AfterEventSourcingState,
            When(AfterBidPlacedEvent)
            .Then(context =>
            {
                context.Saga.Bidder = context.Saga.Bidder;
                context.Saga.LastUpdated = DateTime.UtcNow;
                context.Saga.Id = context.Saga.Id;
                context.Saga.Amount = context.Saga.Amount;
                context.Saga.CorrelationId = context.Saga.CorrelationId;
            })
            .Send(
                new Uri(configuration["QueuePaths:BidFinanceGranting"]),
                context => new BidFinanceGranting(
                context.Saga.Id,
                context.Saga.Bidder,
                context.Saga.Amount,
                context.Saga.CorrelationId
            ))
            .TransitionTo(FinanceGrantedState)
        );
    }
    private void ConfigureFinanceGranted()
    {
        During(FinanceGrantedState,
        //успешно оплатили ставку - получаем последнюю максимальную ставку по указанному аукциону
        When(BidFinanceGrantedEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:GetLastBidPlaced"]),
                context => new GetLastBidPlaced(
                context.Saga.Id,
                context.Saga.CorrelationId
            ))
            .TransitionTo(GetCurrentBidState)
        );
    }

    private void ConfigureBidAuctionPlaced()
    {
        During(GetCurrentBidState,
        //успешно обновили текущую ставку на аукционе - делаем запись о ставке
        When(GetCurrentBidEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
                context.Saga.OldHighBid = context.Message.CurrentHighBid;
            })
            //если нужно посылать одинаковые сообщения в разные консьюмеры (очереди), то соглашения
            //EndpointConvention.Map<тип сообщения> не работают, посылка идет только на один адрес
            //поэтому можно явно указываем адреса для рассылки
            .Send(
                new Uri(configuration["QueuePaths:BidPlacing"]),
                context => new BidPlacing(
                context.Saga.Id,
                context.Saga.Bidder,
                context.Saga.Amount,
                context.Saga.CorrelationId
            ))
            .TransitionTo(BidPlacedState),
        //ошибка при создании записи в аукционе о новой ставке - делаем корректирующую транзакцию для отмены
        //ранее занесенных денег и выход на ошибочное окончание процесса           
        When(BidFinanceGrantedFaultedEvent)
            .Then(context =>
            {
                context.Saga.ErrorMessage = context.Message.Exceptions[0].Message;
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:RollbackBidFinanceGranted"]),
                context => new RollbackBidFinanceGranted(
                context.Saga.Id,
                context.Saga.Bidder,
                context.Saga.Amount,
                context.Saga.CorrelationId
            ))
            .TransitionTo(FaultedState)
        );
    }

    private void ConfigureBidPlaced()
    {
        During(BidPlacedState,
        When(BidPlacedEvent)
            //успешно разместили запись о новой ставке в соответствующем аукционе - размещаем
            //саму запись о новой ставке.    
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
                context.Saga.BidId = context.Message.BidId;
            })
            .Send(
                new Uri(configuration["QueuePaths:BidSearchPlacing"]),
                context => new BidSearchPlacing(
                context.Saga.Id,
                context.Saga.Bidder,
                context.Saga.Amount,
                context.Saga.CorrelationId
            ))
            .TransitionTo(BidSearchPlacedState),
        //ошибка при создании записи о новой ставке - делаем корректирующую транзакции для отмены:
        //-в микросервисе Auction - о новой максимальной ставке
        //-в микроснрвисе Finance о ранее списанных на эту ставку деньгах 
        //и выход на ошибочное окончание процесса           
        When(BidPlacedFaultedEvent)
            .Then(context =>
            {
                context.Saga.ErrorMessage = context.Message.Exceptions[0].Message;
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:RollbackBidFinanceGranted"]),
                context => new RollbackBidFinanceGranted(
                context.Saga.Id,
                context.Saga.Bidder,
                context.Saga.Amount,
                context.Saga.CorrelationId
            ))
            .Send(
                new Uri(configuration["QueuePaths:RollbackBidAuctionPlaced"]),
                context => new RollbackBidAuctionPlaced(
                context.Saga.Id,
                context.Saga.OldHighBid,
                context.Saga.Bidder,
                context.Saga.CorrelationId
            ))
            .TransitionTo(FaultedState)
        );
    }

    private void ConfigureBidSearchPlaced()
    {
        During(BidSearchPlacedState,
        When(BidSearchPlacedEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:BidNotificationProcessing"]),
                context => new BidNotificationProcessing(
                context.Saga.Id,
                context.Saga.Bidder,
                context.Saga.Amount,
                context.Saga.CorrelationId
            ))
            .TransitionTo(UserNotificationSetState),
        //ошибка при создании записи о новой ставке - делаем корректирующую транзакции для отмены:
        //-в микросервисе Auction - о новой максимальной ставке
        //-в микроснрвисе Finance - о ранее списанных на эту ставку деньгах 
        //-в микросервисе Bid - о новой записи - новая ставка
        //и выход на ошибочное окончание процесса           
        When(BidSearchPlacedFaultedEvent)
            .Then(context =>
            {
                context.Saga.ErrorMessage = context.Message.Exceptions[0].Message;
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:RollbackBidFinanceGranted"]),
                context => new RollbackBidFinanceGranted(
                context.Saga.Id,
                context.Saga.Bidder,
                context.Saga.Amount,
                context.Saga.CorrelationId
            ))
            .Send(
                new Uri(configuration["QueuePaths:RollbackBidAuctionPlaced"]),
                context => new RollbackBidAuctionPlaced(
                context.Saga.Id,
                context.Saga.OldHighBid,
                context.Saga.Bidder,
                context.Saga.CorrelationId
            ))
            .Send(
                new Uri(configuration["QueuePaths:RollbackBidPlaced"]),
                context => new RollbackBidPlaced(
                context.Saga.BidId,
                context.Saga.CorrelationId
            ))
            .TransitionTo(FaultedState)
        );
    }

    private void ConfigureBidNotificationProcessed()
    {
        During(UserNotificationSetState,
        When(BidNotificationProcessedEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Activity(p => p.OfType<CommitBidPlacedActivity>())
            .TransitionTo(CommitBidPlacedState)
        );
    }

    private void ConfigureCommitBidPlaced()
    {
        During(CommitBidPlacedState,
        When(CommitBidPlacedEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .TransitionTo(CompletedState)
        );
    }

    private void ConfigureCompleted()
    {
        During(CompletedState);
    }


    private void ConfigureAny()
    {
        DuringAny(
            When(GetBidPlaceStateEvent)
                .Respond(x => x.Saga)
        );
    }

    private void ConfigureFaulted()
    {
        During(FaultedState,
            When(BidFinanceGrantedFaultedEvent)
                .TransitionTo(CompletedState),
            When(BidPlacedFaultedEvent)
                .TransitionTo(CompletedState),
            When(BidSearchPlacedFaultedEvent)
                .TransitionTo(CompletedState)
        );
    }

}