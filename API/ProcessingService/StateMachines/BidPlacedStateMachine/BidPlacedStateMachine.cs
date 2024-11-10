using Common.Contracts;
using MassTransit;
using ProcessingService.Activities.Bid;
using ProcessingService.Activities.Errors;

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
        //Event(() => CommitBidPlacedEvent);
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
            //делаем запись о списании денег, посылаем в лог в EventSourcing
            .Activity(p => p.OfType<FinanceActivity>())
            .TransitionTo(FinanceState)
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
            //рассылка сообщения об ошибках при размещении денег
            .Send(
                new Uri(configuration["QueuePaths:RollbackBidPlaced"]),
                context => new RollbackBidPlaced(
                context.Saga.BidId,
                context.Saga.CorrelationId
            ))
        .Then(context =>
        {
            context.Saga.ErrorMessage = context.Message.Exceptions[0].Message;
            context.Saga.LastUpdated = DateTime.UtcNow;
        })
        .TransitionTo(CompletedState)

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
        //поступил ранее посланный BidFinanceGranting, случилось исключение при его обработке
        //ошибка при создании записи в аукционе о новой ставке - ничего не делаем, переходим на 
        //конечный этап обработки ошибок      
        // When(BidFinanceGrantedFaultedEvent)
        //     .Then(context =>
        //     {
        //         context.Saga.ErrorMessage = context.Message.Exceptions[0].Message;
        //         context.Saga.LastUpdated = DateTime.UtcNow;
        //     })
        //     .Activity(p => p.OfType<CommitErrorFinanceGrantedActivity>())
        //     .TransitionTo(CompletedState)
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
        //поступил ранее посланный BidPlacing, случилось исключение при его обработке
        //ошибка при создании записи в аукционе о новой ставке - делаем корректирующую транзакцию для отмены
        //ранее списанных денег и выход на ошибочное окончание процесса           
        // When(BidPlacedFaultedEvent)
        //     .Then(context =>
        //     {
        //         context.Saga.ErrorMessage = context.Message.Exceptions[0].Message;
        //         context.Saga.LastUpdated = DateTime.UtcNow;
        //     })
        //     //посылаем запрос на отмену списания денег со счета
        //     //FinanceService -> Consumers -> RollbackBidFinanceGrantedConsumer
        //     .Send(
        //         new Uri(configuration["QueuePaths:RollbackBidFinanceGranted"]),
        //         context => new RollbackBidFinanceGranted(
        //         context.Saga.AuctionId,
        //         context.Saga.Bidder,
        //         context.Saga.Amount,
        //         context.Saga.CorrelationId
        //     ))
        //     .Activity(p => p.OfType<CommitErrorBidPlacedActivity>())
        //     .TransitionTo(CompletedState)
        // );
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
        //ошибка при создании записи о новой ставке, исключение при обработке BidSearchPlacing - 
        //делаем корректирующую транзакции для отмены:
        //-в микросервисе Finance - о ранее списанных на эту ставку деньгах 
        //-в микросервисе Bid - о новой записи - новая ставка
        //и выход на ошибочное окончание процесса           
        // When(BidSearchPlacedFaultedEvent)
        //     .Then(context =>
        //     {
        //         context.Saga.ErrorMessage = context.Message.Exceptions[0].Message;
        //         context.Saga.LastUpdated = DateTime.UtcNow;
        //     })
        //     //отменяем списание денег
        //     //FinanceService -> Consumers -> RollbackBidFinanceGrantedConsumer
        //     .Send(
        //         new Uri(configuration["QueuePaths:RollbackBidFinanceGranted"]),
        //         context => new RollbackBidFinanceGranted(
        //         context.Saga.AuctionId,
        //         context.Saga.Bidder,
        //         context.Saga.Amount,
        //         context.Saga.CorrelationId
        //     ))
        //     //отменяем запись о новой ставке
        //     .Send(
        //         new Uri(configuration["QueuePaths:RollbackBidPlaced"]),
        //         context => new RollbackBidPlaced(
        //         context.Saga.BidId,
        //         context.Saga.CorrelationId
        //     ))
        //     .Activity(p => p.OfType<CommitErrorBidSearchPlaceActivity>())
        //     .TransitionTo(CompletedState)
        // );
    }

    private void ConfigureESCommitState()
    {
        During(ESCommitState,
        When(CommitEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .TransitionTo(CompletedState));
    }

    //private void ConfigureBidNotificationProcessed()
    //{
    //поступила BidNotificationProcessed
    // During(UserNotificationSetState,
    // When(BidNotificationProcessedEvent)
    // //успешно создали рассылку уведомлений
    //     .Then(context =>
    //     {
    //         context.Saga.LastUpdated = DateTime.UtcNow;
    //     })

    //     .TransitionTo(CompletedState));
    // When(BidNotificationFaultedEvent)
    // //ошибка рассылки уведомлений
    // //поступил BidNotificationProcessing - ничего не корректируем, просто переходим на обработку ошибок
    //     .Then(context =>
    //     {
    //         context.Saga.ErrorMessage = context.Message.Exceptions[0].Message;
    //         context.Saga.LastUpdated = DateTime.UtcNow;
    //     })
    //     .Activity(p => p.OfType<CommitErrorNotificationActivity>())
    //     .TransitionTo(CompletedState)
    // );
    // }

    // private void ConfigureCommitBidPlaced()
    // {
    //     //поступила CommitBidPlacedContract
    //     During(CommitBidPlacedState,
    //     When(CommitBidPlacedEvent)
    //     //успешно прошла фиксация новой ставки в EventSourcing
    //         .Then(context =>
    //         {
    //             context.Saga.LastUpdated = DateTime.UtcNow;
    //         })
    //         .TransitionTo(CompletedState),
    //     When(ErrorBidEventSourcingCommitEvent)
    //     //ошибка фиксации новой ставки в EventSourcing
    //     //поступил CommitBidPlacedErrorContract - ничего не корректируем, 
    //     //фиксируем в логе Саги эту ошибку, уже ничего не сделать
    //         .Then(context =>
    //         {
    //             context.Saga.ErrorMessage = $"Ошибка подтверждения записи в EventSourcing - {context.Message.ExceptionItem.Message}";
    //             context.Saga.LastUpdated = DateTime.UtcNow;
    //         })
    //         .TransitionTo(CompletedState)
    //     );

    // }

    private void ConfigureCompleted()
    {
        During(CompletedState);
    }

}