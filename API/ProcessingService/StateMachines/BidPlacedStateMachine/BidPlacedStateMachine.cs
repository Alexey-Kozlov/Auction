using Common.Contracts;
using MassTransit;

namespace ProcessingService.StateMachines.BidPlacedStateMachine;
public class BidPlacedStateMachine : MassTransitStateMachine<BidPlacedState>
{
    public State FinanceGrantedState { get; }
    public State BidAuctionPlacedState { get; }
    public State BidPlacedState { get; }
    public State BidSearchPlacedState { get; }
    public State UserNotificationSetState { get; }
    public State CompletedState { get; }
    public State FaultedState { get; }

    public Event<RequestBidPlace> RequestBidPlaceEvent { get; }
    public Event<BidFinanceGranted> BidFinanceGrantedEvent { get; }
    public Event<BidAuctionPlaced> BidAuctionPlacedEvent { get; }
    public Event<BidPlaced> BidPlacedEvent { get; }
    public Event<BidSearchPlaced> BidSearchPlacedEvent { get; }
    public Event<BidNotificationProcessed> BidNotificationProcessedEvent { get; }
    public Event<GetBidPlaceState> GetBidPlaceStateEvent { get; }
    public Event<Fault<BidFinanceGranting>> BidFinanceGrantedFaultedEvent { get; }
    public Event<Fault<BidAuctionPlacing>> BidAuctionPlacedFaultedEvent { get; }
    public Event<Fault<BidPlacing>> BidPlacedFaultedEvent { get; }
    public Event<Fault<BidSearchPlacing>> BidSearchPlacedFaultedEvent { get; }

    public BidPlacedStateMachine()
    {
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureFinanceGranted();
        ConfigureBidAuctionPlaced();
        ConfigureBidPlaced();
        ConfigureBidSearchPlaced();
        ConfigureBidNotificationProcessed();
        ConfigureCompleted();
        ConfigureAny();
        ConfigureFaulted();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestBidPlaceEvent);
        Event(() => BidFinanceGrantedEvent);
        Event(() => BidAuctionPlacedEvent);
        Event(() => BidPlacedEvent);
        Event(() => BidSearchPlacedEvent);
        Event(() => BidNotificationProcessedEvent);
        Event(() => GetBidPlaceStateEvent);
        Event(() => BidFinanceGrantedFaultedEvent, x => x.CorrelateById(
            context => context.Message.Message.CorrelationId));
        Event(() => BidAuctionPlacedFaultedEvent, x => x.CorrelateById(
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
            })
            .Send(context => new BidFinanceGranting(
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
        //успешно оплатили ставку - делаем запись о ставке в соответствующем аукционе
        When(BidFinanceGrantedEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(context => new BidAuctionPlacing(
                context.Saga.Id,
                context.Saga.Bidder,
                context.Saga.Amount,
                context.Saga.CorrelationId
            ))
            .TransitionTo(BidAuctionPlacedState),
        //если ошибка при оплате - например, превышение имеющегося количества денег - 
        //нет нужды делать корректироующую транзакцию, так как данные по оплате не заносились
        //делаем запись в истории, делаем уведомление пользователю и переходим на конец процесса
        When(BidFinanceGrantedFaultedEvent)
            .Then(context =>
            {
                context.Saga.ErrorMessage = context.Message.Exceptions[0].Message;
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .TransitionTo(FaultedState)
        );
    }

    private void ConfigureBidAuctionPlaced()
    {
        During(BidAuctionPlacedState,
        //успешно обновили текущую ставку на аукционе - делаем запись о ставке
        When(BidAuctionPlacedEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
                context.Saga.OldHighBid = context.Message.OldHighBid;
            })
            //если нужно посылать одинаковые сообщения в разные консьюмеры (очереди), то соглашения
            //EndpointConvention.Map<тип сообщения> не работают, посылка идет только на одтин адрес
            //поэтому в этом случае явно указываем адреса для рассылки
            .Send(new Uri("queue:bids-bid-placing"), context => new BidPlacing(
                context.Saga.Id,
                context.Saga.Bidder,
                context.Saga.Amount,
                context.Saga.CorrelationId
            ))
            .Send(new Uri("queue:metrics-bid-added-metrics"), context => new BidPlacing(
                context.Saga.Id,
                context.Saga.Bidder,
                context.Saga.Amount,
                context.Saga.CorrelationId
            ))
            .TransitionTo(BidPlacedState),
        //ошибка при создании записи в аукционе о новой ставке - делаем корректирующую транзакцию для отмены
        //ранее занесенных денег и выход на ошибочное окончание процесса           
        When(BidAuctionPlacedFaultedEvent)
            .Then(context =>
            {
                context.Saga.ErrorMessage = context.Message.Exceptions[0].Message;
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(context => new RollbackBidFinanceGranted(
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
            .Send(context => new BidSearchPlacing(
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
            .Send(context => new RollbackBidFinanceGranted(
                context.Saga.Id,
                context.Saga.Bidder,
                context.Saga.Amount,
                context.Saga.CorrelationId
            ))
            .Send(context => new RollbackBidAuctionPlaced(
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
            .Send(context => new BidNotificationProcessing(
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
            .Send(context => new RollbackBidFinanceGranted(
                context.Saga.Id,
                context.Saga.Bidder,
                context.Saga.Amount,
                context.Saga.CorrelationId
            ))
            .Send(context => new RollbackBidAuctionPlaced(
                context.Saga.Id,
                context.Saga.OldHighBid,
                context.Saga.Bidder,
                context.Saga.CorrelationId
            ))
            .Send(context => new RollbackBidPlaced(
                context.Saga.BidId,
                context.Saga.CorrelationId
            ))
            .TransitionTo(FaultedState)
        );
    }

    private void ConfigureBidNotificationProcessed()
    {
        During(UserNotificationSetState,
        //успешно оплатили ставку - переходим к этапу создания заявки        
        When(BidNotificationProcessedEvent)
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
            When(BidAuctionPlacedFaultedEvent)
                .TransitionTo(CompletedState),
            When(BidPlacedFaultedEvent)
                .TransitionTo(CompletedState),
            When(BidSearchPlacedFaultedEvent)
                .TransitionTo(CompletedState)
        );
    }

}