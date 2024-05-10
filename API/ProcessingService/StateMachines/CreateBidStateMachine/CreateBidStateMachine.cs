using Contracts;
using MassTransit;

namespace ProcessingService.StateMachines.CreateBidStateMachine;
public class CreateBidStateMachine : MassTransitStateMachine<CreateBidState>
{
    public State AcceptedState { get; }
    public State UserNotificationSetState { get; }
    public State FinanceGrantedState { get; }
    public State CompletedState { get; }
    public State FaultedState { get; }

    public Event<RequestProcessingBidStart> RequestProcessingBidStartEvent { get; }
    public Event<FinanceGranted> FinanceGrantedEvent { get; }
    public Event<BidPlaced> BidPlacedEvent { get; }
    public Event<UserNotificationAdded> UserNotificationAddedEvent { get; }
    public Event<GetProcessingBidState> GetProcessingBidStateEvent { get; }
    public Event<Fault<UserNotificationSet>> UserNotificationSetFaulted { get; }
    public Event<Fault<RequestFinanceDebitAdd>> FinanceGrantedFaulted { get; }
    public Event<Fault<RequestBidPlace>> BidPlacedFaulted { get; }

    public CreateBidStateMachine()
    {
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureAccepted();
        ConfigureUserNotificationSet();
        ConfigureFinanceGranted();
        ConfigureCompleted();
        ConfigureAny();
        ConfigureFaulted();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestProcessingBidStartEvent);
        Event(() => FinanceGrantedEvent);
        Event(() => BidPlacedEvent);
        Event(() => UserNotificationAddedEvent);
        Event(() => GetProcessingBidStateEvent);
        Event(() => UserNotificationSetFaulted, x => x.CorrelateById(
            context => context.Message.Message.CorrelationId));
        Event(() => FinanceGrantedFaulted, x => x.CorrelateById(
            context => context.Message.Message.CorrelationId));
        Event(() => BidPlacedFaulted, x => x.CorrelateById(
            context => context.Message.Message.CorrelationId));
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestProcessingBidStartEvent)
            .Then(context =>
            {
                context.Saga.Bidder = context.Message.Bidder;
                context.Saga.LastUpdated = DateTime.UtcNow;
                context.Saga.AuctionId = context.Message.AuctionId;
                context.Saga.Amount = context.Message.Amount;
                context.Saga.CorrelationId = context.Message.CorrelationId;
            })
            //добавляем пользователя, кто сделал ставку - в рассылку для получения уведомлений от этого аукциона
            .Send(context => new UserNotificationSet(
                context.Message.AuctionId,
                context.Message.Bidder,
                context.Message.CorrelationId
            ))
            .TransitionTo(AcceptedState)
        );
    }

    private void ConfigureAccepted()
    {
        During(AcceptedState,
        When(UserNotificationAddedEvent)
            //успешно выполнился этап установки уведомления пользователя - начинаем этап оплаты денег        
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(context => new RequestFinanceDebitAdd(
                context.Saga.Amount,
                context.Saga.AuctionId,
                context.Saga.Bidder,
                context.Saga.CorrelationId
            ))
            .TransitionTo(UserNotificationSetState),
        //ошибка этапа установки уведомления пользователя - ничего не корректируем, переходим на конец            
        When(UserNotificationSetFaulted)
            .Then(context =>
            {
                context.Saga.ErrorMessage = context.Message.Exceptions[0].Message;
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .TransitionTo(FaultedState)
        );
    }

    private void ConfigureUserNotificationSet()
    {
        During(UserNotificationSetState,
        //успешно оплатили ставку - переходим к этапу создания заявки        
        When(FinanceGrantedEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(context => new RequestBidPlace(
                context.Saga.AuctionId,
                context.Saga.Bidder,
                context.Saga.Amount,
                context.Saga.CorrelationId
            ))
            .TransitionTo(FinanceGrantedState),
        //ошибка оплаты ставки - ничего не корректируем, переходим на конец            
        When(FinanceGrantedFaulted)
            .Then(context =>
            {
                context.Saga.ErrorMessage = context.Message.Exceptions[0].Message;
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .TransitionTo(FaultedState)
        );
    }

    private void ConfigureFinanceGranted()
    {
        During(FinanceGrantedState,
        //успешно разместили ставку - конец процесса        
        When(BidPlacedEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .TransitionTo(CompletedState),
        //ошибка размещения ставки - начинаем процесс отмены резервирования денег, корректирующая транзакция.            
        When(BidPlacedFaulted)
            .Then(context =>
            {
                context.Saga.ErrorMessage = context.Message.Exceptions[0].Message;
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(context => new RollbackFinanceDebitAdd(
                context.Saga.Amount,
                context.Saga.AuctionId,
                context.Saga.Bidder,
                context.Saga.CorrelationId
            ))
            .TransitionTo(FaultedState)
        );
    }

    private void ConfigureCompleted()
    {
        During(CompletedState);
    }


    private void ConfigureAny()
    {
        DuringAny(
            When(GetProcessingBidStateEvent)
                .Respond(x => x.Saga)
        );
    }

    private void ConfigureFaulted()
    {
        During(FaultedState,
            When(FinanceGrantedFaulted)
                .TransitionTo(CompletedState),
            When(BidPlacedFaulted)
                .TransitionTo(CompletedState),
            When(UserNotificationSetFaulted)
                .TransitionTo(CompletedState)
        );
    }

}