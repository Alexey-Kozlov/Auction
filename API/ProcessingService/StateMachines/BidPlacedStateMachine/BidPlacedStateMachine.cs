using Contracts;
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
    public Event<Fault<BidFinanceGranted>> BidFinanceGrantedFaulted { get; }
    public Event<Fault<BidAuctionPlaced>> BidAuctionPlacedFaulted { get; }
    public Event<Fault<BidPlaced>> BidPlacedFaulted { get; }

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
        Event(() => BidFinanceGrantedFaulted, x => x.CorrelateById(
            context => context.Message.Message.CorrelationId));
        Event(() => BidAuctionPlacedFaulted, x => x.CorrelateById(
            context => context.Message.Message.CorrelationId));
        Event(() => BidPlacedFaulted, x => x.CorrelateById(
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
                context.Saga.AuctionId = context.Message.AuctionId;
                context.Saga.Amount = context.Message.Amount;
                context.Saga.CorrelationId = context.Message.CorrelationId;
            })
            .Send(context => new BidFinanceGranting(
                context.Saga.AuctionId,
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
                context.Saga.AuctionId,
                context.Saga.Amount,
                context.Saga.CorrelationId
            ))
            .TransitionTo(BidAuctionPlacedState),
        //ошибка оплаты ставки - выход на ошибочное окончание процесса
        When(BidFinanceGrantedFaulted)
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
            })
            .Send(context => new BidPlacing(
                context.Saga.AuctionId,
                context.Saga.Bidder,
                context.Saga.Amount,
                context.Saga.CorrelationId
            ))
            .TransitionTo(BidPlacedState)
        //ошибка размещения ставки - начинаем процесс отмены резервирования денег, корректирующая транзакция.            
        // When(BidPlacedFaulted)
        //     .Then(context =>
        //     {
        //         context.Saga.ErrorMessage = context.Message.Exceptions[0].Message;
        //         context.Saga.LastUpdated = DateTime.UtcNow;
        //     })
        //     .Send(context => new RollbackFinanceDebitAdd(
        //         context.Saga.Amount,
        //         context.Saga.AuctionId,
        //         context.Saga.Bidder,
        //         context.Saga.CorrelationId
        //     ))
        //     .TransitionTo(FaultedState)
        );
    }

    private void ConfigureBidPlaced()
    {
        During(BidPlacedState,
        When(BidPlacedEvent)
            //успешно разместили новую заявку -       
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(context => new BidSearchPlacing(
                context.Saga.AuctionId,
                context.Saga.Amount,
                context.Saga.CorrelationId
            ))
            .TransitionTo(BidSearchPlacedState)
        //ошибка этапа установки уведомления пользователя - ничего не корректируем, переходим на конец            
        // When(UserNotificationSetFaulted)
        //     .Then(context =>
        //     {
        //         context.Saga.ErrorMessage = context.Message.Exceptions[0].Message;
        //         context.Saga.LastUpdated = DateTime.UtcNow;
        //     })
        //     .TransitionTo(FaultedState)
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
                context.Saga.AuctionId,
                context.Saga.Bidder,
                context.Saga.Amount,
                context.Saga.CorrelationId
            ))
            .TransitionTo(UserNotificationSetState)
        //ошибка этапа установки уведомления пользователя - ничего не корректируем, переходим на конец            
        // When(UserNotificationSetFaulted)
        //     .Then(context =>
        //     {
        //         context.Saga.ErrorMessage = context.Message.Exceptions[0].Message;
        //         context.Saga.LastUpdated = DateTime.UtcNow;
        //     })
        //     .TransitionTo(FaultedState)
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
        // During(FaultedState,
        //     When(FinanceGrantedFaulted)
        //         .TransitionTo(CompletedState),
        //     When(BidPlacedFaulted)
        //         .TransitionTo(CompletedState),
        //     When(UserNotificationSetFaulted)
        //         .TransitionTo(CompletedState)
        // );
    }

}