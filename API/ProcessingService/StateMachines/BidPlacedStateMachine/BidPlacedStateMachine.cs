using Common.Contracts;
using MassTransit;
using ProcessingService.Activities.BidPlaced;
using ProcessingService.Activities.Errors;

namespace ProcessingService.StateMachines.BidPlacedStateMachine;
public class BidPlacedStateMachine : MassTransitStateMachine<BidPlacedState>
{
    public State GetCurrentBidState { get; }
    public State FinanceGrantedState { get; }
    public State BidPlacedState { get; }
    public State BidSearchPlacedState { get; }
    public State UserNotificationSetState { get; }
    public State CommitBidPlacedState { get; }
    public State CompletedState { get; }

    public Event<RequestBidPlace> RequestBidPlaceEvent { get; }
    public Event<AfterBidPlacedContract> AfterBidPlacedEventSourcingEvent { get; }
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
    public Event<Fault<BidNotificationProcessing>> BidNotificationFaultedEvent { get; }
    public Event<CommitBidPlacedErrorContract> ErrorBidEventSourcingCommitEvent { get; }
    private IConfiguration configuration { get; }


    public BidPlacedStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureGetLastBid();
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureFinanceGranting();
        ConfigureFinanceBidPlaced();
        ConfigureBidPlaced();
        ConfigureBidSearchPlaced();
        ConfigureBidNotificationProcessed();
        ConfigureCommitBidPlaced();
        ConfigureCompleted();
        ConfigureAny();
    }

    private void ConfigureEvents()
    {
        Event(() => RequestBidPlaceEvent, p =>
        {
            //ВАЖНО! Для эвента в секции Initially - нужно указать этот флаг,
            //иначе первый раз после рестарта не срабатывает событие
            p.InsertOnInitial = true;
        });
        Event(() => AfterBidPlacedEventSourcingEvent);
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
        Event(() => BidNotificationFaultedEvent, x => x.CorrelateById(
            context => context.Message.Message.CorrelationId));
        Event(() => ErrorBidEventSourcingCommitEvent);
    }
    private void ConfigureInitialState()
    {
        //поступил запрос на создание ставки
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
            //посылаем поступившую инфу в лог в EventSourcing
            .Activity(p => p.OfType<BidPlacedActivity>())
            //.Publish(context => new AfterBidPlacedContract(context.Saga.CorrelationId))
            .TransitionTo(GetCurrentBidState)

        );
    }

    private void ConfigureGetLastBid()
    {
        //поступил AfterBidPlacedContract - после создания записи в логе EventSourcing
        During(GetCurrentBidState,
        When(AfterBidPlacedEventSourcingEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //получаем максимальную сделанную ставку по указанному аукциону
            .Send(
                new Uri(configuration["QueuePaths:GetLastBidPlaced"]),
                context => new GetLastBidPlaced(
                context.Saga.Id,
                context.Saga.CorrelationId
            ))
            .TransitionTo(FinanceGrantedState)
        );
    }

    private void ConfigureFinanceGranting()
    {
        //поступил GetCurrentBid - максимальная ставка по данному аукциону
        During(FinanceGrantedState,
            When(GetCurrentBidEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
                context.Saga.OldHighBid = context.Message.CurrentHighBid;
            })
            //посылаем запрос на списание денег со счета, в 
            //ProcessingFinance -> Consumers -> BidFinanceGrantingConsumer
            .Send(
                new Uri(configuration["QueuePaths:BidFinanceGranting"]),
                context => new BidFinanceGranting(
                context.Saga.Id,
                context.Saga.Bidder,
                context.Saga.Amount,
                context.Saga.CorrelationId
            ))
            .TransitionTo(BidPlacedState)
        );
    }


    private void ConfigureFinanceBidPlaced()
    {
        //поступил BidFinanceGranted - получили ответ по списанию денег со счета
        During(BidPlacedState,
        //успешно списали деньги по ставке со счета - делаем запись о ставке
        When(BidFinanceGrantedEvent)
            .Then(context =>
            {
                context.Saga.Bidder = context.Saga.Bidder;
                context.Saga.LastUpdated = DateTime.UtcNow;
                context.Saga.Id = context.Saga.Id;
                context.Saga.Amount = context.Saga.Amount;
                context.Saga.CorrelationId = context.Saga.CorrelationId;
            })
            //посылаем запрос на создание новой ставки в Bids
            //BiddingService -> BidPlacingConsumers
            .Send(
                new Uri(configuration["QueuePaths:BidPlacing"]),
                context => new BidPlacing(
                context.Saga.Id,
                context.Saga.Bidder,
                context.Saga.Amount,
                context.Saga.CorrelationId
            ))
            .TransitionTo(BidPlacedState),
        //поступил ранее посланный BidFinanceGranting, случилось исключение при его обработке
        //ошибка при создании записи в аукционе о новой ставке - ничего не делаем, переходим на 
        //конечный этап обработки ошибок      
        When(BidFinanceGrantedFaultedEvent)
            .Then(context =>
            {
                context.Saga.ErrorMessage = context.Message.Exceptions[0].Message;
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Activity(p => p.OfType<CommitErrorFinanceGrantedActivity>())
            .TransitionTo(CompletedState)
        );
    }

    private void ConfigureBidPlaced()
    {
        //поступил BidPlaced
        During(BidPlacedState,
        When(BidPlacedEvent)
            //успешно разместили запись о новой ставке в соответствующем аукционе - размещаем
            //запись о новой ставке в Search для публикации  
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
                context.Saga.BidId = context.Message.BidId;
            })
            //посылаем запрос на публикацию новой ставки в
            //SearchService -> Consumers -> BidSearchPlacingConsumer
            .Send(
                new Uri(configuration["QueuePaths:BidSearchPlacing"]),
                context => new BidSearchPlacing(
                context.Saga.Id,
                context.Saga.Bidder,
                context.Saga.Amount,
                context.Saga.CorrelationId
            ))
            .TransitionTo(BidSearchPlacedState),
        //поступил ранее посланный BidPlacing, случилось исключение при его обработке
        //ошибка при создании записи в аукционе о новой ставке - делаем корректирующую транзакцию для отмены
        //ранее списанных денег и выход на ошибочное окончание процесса           
        When(BidPlacedFaultedEvent)
            .Then(context =>
            {
                context.Saga.ErrorMessage = context.Message.Exceptions[0].Message;
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //посылаем запрос на отмену списания денег со счета
            //FinanceService -> Consumers -> RollbackBidFinanceGrantedConsumer
            .Send(
                new Uri(configuration["QueuePaths:RollbackBidFinanceGranted"]),
                context => new RollbackBidFinanceGranted(
                context.Saga.Id,
                context.Saga.Bidder,
                context.Saga.Amount,
                context.Saga.CorrelationId
            ))
            .Activity(p => p.OfType<CommitErrorBidPlacedActivity>())
            .TransitionTo(CompletedState)
        );
    }

    private void ConfigureBidSearchPlaced()
    {
        //поступил BidSearchPlaced
        During(BidSearchPlacedState,
        When(BidSearchPlacedEvent)
        //успешно опубликовали новую ставку в Search
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //посылаем запрос на уведомление о новой ставке
            //NotificationService -> Consumers -> BidNotificationProcessingConsumer
            .Send(
                new Uri(configuration["QueuePaths:BidNotificationProcessing"]),
                context => new BidNotificationProcessing(
                context.Saga.Id,
                context.Saga.Bidder,
                context.Saga.Amount,
                context.Saga.CorrelationId
            ))
            .TransitionTo(UserNotificationSetState),
        //ошибка при создании записи о новой ставке, исключение при обработке BidSearchPlacing - 
        //делаем корректирующую транзакции для отмены:
        //-в микросервисе Finance - о ранее списанных на эту ставку деньгах 
        //-в микросервисе Bid - о новой записи - новая ставка
        //и выход на ошибочное окончание процесса           
        When(BidSearchPlacedFaultedEvent)
            .Then(context =>
            {
                context.Saga.ErrorMessage = context.Message.Exceptions[0].Message;
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //отменяем списание денег
            //FinanceService -> Consumers -> RollbackBidFinanceGrantedConsumer
            .Send(
                new Uri(configuration["QueuePaths:RollbackBidFinanceGranted"]),
                context => new RollbackBidFinanceGranted(
                context.Saga.Id,
                context.Saga.Bidder,
                context.Saga.Amount,
                context.Saga.CorrelationId
            ))
            //отменяем запись о новой ставке
            .Send(
                new Uri(configuration["QueuePaths:RollbackBidPlaced"]),
                context => new RollbackBidPlaced(
                context.Saga.BidId,
                context.Saga.CorrelationId
            ))
            .Activity(p => p.OfType<CommitErrorBidSearchPlaceActivity>())
            .TransitionTo(CompletedState)
        );
    }

    private void ConfigureBidNotificationProcessed()
    {
        //поступила BidNotificationProcessed
        During(UserNotificationSetState,
        When(BidNotificationProcessedEvent)
        //успешно создали рассылку уведомлений
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Activity(p => p.OfType<CommitBidPlacedActivity>())
            .TransitionTo(CommitBidPlacedState),
        When(BidNotificationFaultedEvent)
        //ошибка рассылки уведомлений
        //поступил BidNotificationProcessing - ничего не корректируем, просто переходим на обработку ошибок
            .Then(context =>
            {
                context.Saga.ErrorMessage = context.Message.Exceptions[0].Message;
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Activity(p => p.OfType<CommitErrorNotificationActivity>())
            .TransitionTo(CompletedState)
        );
    }

    private void ConfigureCommitBidPlaced()
    {
        //поступила CommitBidPlacedContract
        During(CommitBidPlacedState,
        When(CommitBidPlacedEvent)
        //успешно прошла фиксация новой ставки в EventSourcing
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .TransitionTo(CompletedState),
        When(ErrorBidEventSourcingCommitEvent)
        //ошибка фиксации новой ставки в EventSourcing
        //поступил CommitBidPlacedErrorContract - ничего не корректируем, 
        //фиксируем в логе Саги эту ошибку, уже ничего не сделать
            .Then(context =>
            {
                context.Saga.ErrorMessage = $"Ошибка подтверждения записи в EventSourcing - {context.Message.ExceptionItem.Message}";
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

}