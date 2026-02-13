using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Bid;
using Common.Contracts.Communication;
using Common.Contracts.Finance;
using Common.Contracts.Image;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using MassTransit;
using ProcessingService.Activities.AuctionDelete;

namespace ProcessingService.StateMachines.DeleteAuctionStateMachine;

public class DeleteAuctionStateMachine : MassTransitStateMachine<DeleteAuctionState>
{
    public State FinanceState { get; }
    public State BidState { get; }
    public State GatewayState { get; }
    public State ImageState { get; }
    public State SearchState { get; }
    public State CommunicationState { get; }
    public State ElkState { get; }
    public State NotificationState { get; }
    public State PreCommitState { get; }
    public State CommitState { get; }
    public State CompleteState { get; }


    public Event<RequestAuctionDelete> RequestEvent { get; }
    public Event<ESLogAuctionDeleted> EsLogEvent { get; }
    public Event<AuctionDeletedBid> BidEvent { get; }
    public Event<AuctionDeletedGateway> GatewayEvent { get; }
    public Event<AuctionDeletedImage> ImageEvent { get; }
    public Event<AuctionDeletedSearch> SearchEvent { get; }
    public Event<AuctionDeletedElk> ElkEvent { get; }
    public Event<AuctionDeletedNotification> NotificationEvent { get; }
    public Event<AuctionDeletedCommunication> CommunicationEvent { get; }
    public Event<AuctionDeletedNotificationEvent> NotificationUIEvent { get; }
    public Event<AuctionDeleteESCommit> CommitEvent { get; }
    public Event<BaseServiceError> FaultEvent { get; }
    public Event<Fault<ESLogAuctionDeleted>> FaultEsLogEvent { get; }
    public Event<Fault<AuctionDeletedBid>> FaultBidEvent { get; }
    public Event<Fault<AuctionDeletedGateway>> FaultGatewayEvent { get; }
    public Event<Fault<AuctionDeletedImage>> FaultImageEvent { get; }
    public Event<Fault<AuctionDeletedSearch>> FaultSearchEvent { get; }
    public Event<Fault<AuctionDeletedElk>> FaultElkEvent { get; }
    public Event<Fault<AuctionDeletedNotification>> FaultNotificationEvent { get; }
    public Event<Fault<AuctionDeletedCommunication>> FaultCommunicationEvent { get; }
    public Event<Fault<AuctionDeletedNotificationEvent>> FaultNotificationUIEvent { get; }
    public Event<Fault<AuctionDeleteESCommit>> FaultCommitEvent { get; }
    private IConfiguration configuration { get; }
    public object locker = new();

    public DeleteAuctionStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureFinanceState();
        ConfigureBidState();
        ConfigureGatewayState();
        ConfigureImageState();
        ConfigureSearchState();
        ConfigureCommunicationState();
        ConfigureELKState();
        ConfigureNotificationState();
        ConfigurePreCommitState();
        ConfigureCommitState();
        ConfigureCompleteState();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestEvent, p =>
        {
            p.InsertOnInitial = true;
        });
        Event(() => EsLogEvent);
        Event(() => BidEvent);
        Event(() => GatewayEvent);
        Event(() => ImageEvent);
        Event(() => SearchEvent);
        Event(() => ElkEvent);
        Event(() => NotificationEvent);
        Event(() => CommunicationEvent);
        Event(() => NotificationUIEvent);
        Event(() => CommitEvent);
        Event(() => FaultEvent);
        Event(() => FaultEsLogEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultBidEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultGatewayEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultImageEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultSearchEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultElkEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultNotificationEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultCommunicationEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultNotificationUIEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultCommitEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestEvent)
            .Then(context =>
            {
                context.Saga.ItemId = context.Message.ItemId;
                context.Saga.UserLogin = context.Message.UserLogin;
                context.Saga.IsError = false;
                context.Saga.CommitCounter = 8;
            })
            //посылаем через Кафку, выполнение всех операций в ES лог для удаления аукциона:
            // - Удаление записей по деньгам в сервисе FinanceService
            // - Удаление всех ставок в сервисе BiddingService
            // - Удаление записи в сервисе SearchService
            // - Удаление всех записей в сервисе NotificationService
            .Activity(p => p.OfType<ESLogActivity>())
            .TransitionTo(FinanceState)
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
            //Удаление записи (деньги на ставку, если есть) в сервисе FinanceService
            .Send(
                new Uri(configuration["QueuePaths:FinanceConsumer"]),
                context => new DataForProcessingServicesList<FinanceItem>
                {
                    DataObjects = context.Message.DataItems.DataObjects.Where(p => p.DataType == "FinanceItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionDeletedBid"
                })
            .TransitionTo(BidState),
        //обрабатываем ошибки из сервиса EventSourcingService            
        When(FaultEsLogEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionMessage = context.Message.Message.ErrorExceptionMessage,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
        .TransitionTo(PreCommitState)
        );
    }

    private void ConfigureBidState()
    {
        During(BidState,
        When(BidEvent)
            //Удаление записей (ставка, если есть) в сервисе BiddingService
            .Send(
                new Uri(configuration["QueuePaths:BidConsumer"]),
                context => new DataForProcessingServicesList<BidItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "BidItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionDeletedGateway"
                })
            .TransitionTo(GatewayState),
        //обрабатываем ошибки из сервиса FinanceService            
        When(FaultBidEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionMessage = context.Message.Message.ErrorExceptionMessage,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
        .TransitionTo(PreCommitState)
        );
    }

    private void ConfigureGatewayState()
    {
        During(GatewayState,
        When(GatewayEvent)
            //Удаление аукционв из кеша в сервисе GatewayService
            .Send(
                new Uri(configuration["QueuePaths:GatewayConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "AuctionItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionDeletedImage"
                })
            .TransitionTo(ImageState),
        //обрабатываем ошибки из сервиса BiddingService            
        When(FaultGatewayEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionMessage = context.Message.Message.ErrorExceptionMessage,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
        .TransitionTo(PreCommitState)
        );
    }

    private void ConfigureImageState()
    {
        During(ImageState,
        When(ImageEvent)
            //Удаление изображения аукциона в сервисе ImageService
            .Send(
                new Uri(configuration["QueuePaths:ImageConsumer"]),
                context => new DataForProcessingServicesList<ImageDTO>
                {
                    DataObjects = new List<DataForProcessingService>
                    {
                    //передаем тип операции
                        new DataForProcessingService
                        {
                            CRUD = CRUD.Delete,
                            Data = "CRUD",
                            MessagePartId = context.Saga.ItemId.Value
                        }
                    },
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionDeletedSearch"
                })
            .TransitionTo(SearchState),
        //обрабатываем ошибки из сервиса GatewayService            
        When(FaultImageEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionMessage = context.Message.Message.ErrorExceptionMessage,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
        .TransitionTo(PreCommitState)
        );
    }

    private void ConfigureSearchState()
    {
        During(SearchState,
        When(SearchEvent)
            //Удаление аукциона в сервисе SearchService
            .Send(
                new Uri(configuration["QueuePaths:SearchConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "AuctionItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionDeletedCommunication"
                })
            .TransitionTo(CommunicationState),
        //обрабатываем ошибки из сервиса ImageService            
        When(FaultSearchEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionMessage = context.Message.Message.ErrorExceptionMessage,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
        .TransitionTo(PreCommitState)
        );
    }

    private void ConfigureCommunicationState()
    {
        During(CommunicationState,
        When(CommunicationEvent)
            //Удаление записей (если есть) в сервисе CommunicationService
            .Send(
                new Uri(configuration["QueuePaths:CommunicationsConsumer"]),
                context => new DataForProcessingServicesList<CommunicationItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "CommunicationItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionDeletedElk"
                })
            .TransitionTo(ElkState),
        When(FaultCommunicationEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionMessage = context.Message.Message.ErrorExceptionMessage,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
        .TransitionTo(PreCommitState)
        );
    }

    private void ConfigureELKState()
    {
        During(ElkState,
        When(ElkEvent)
            //Удаление аукциона и сообщений из поиска в сервисе ElasticSearchService,
            .Send(
                new Uri(configuration["QueuePaths:ElkConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "AuctionItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionDeletedNotification"
                })
            .TransitionTo(NotificationState),
        //обрабатываем ошибки из сервиса SearchService            
        When(FaultElkEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionMessage = context.Message.Message.ErrorExceptionMessage,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
        .TransitionTo(PreCommitState)
        );
    }

    private void ConfigureNotificationState()
    {
        During(NotificationState,
        When(NotificationEvent)
            //Удаление уведомлений (если есть) в сервисе NotificationService
            .Send(
                new Uri(configuration["QueuePaths:AuctionEditConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "NotifyItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionDeleteESCommit",
                    Props = ""
                })
            .TransitionTo(PreCommitState),
        //обрабатываем ошибки из сервиса ElkService            
        When(FaultNotificationEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionMessage = context.Message.Message.ErrorExceptionMessage,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
        .TransitionTo(PreCommitState)
        );
    }

    /*промежуточный этап перед подтверждением/откатом транзакции
    на входе события:
    - BaseServiceError - событие ошибок от предыдущих этапов
    - Fault<AuctionDeleteESCommit> - событие ошибки предыдущего этапа
    - AuctionDeleteESCommit - событие правильного выполнения предыдущего этапа
    на выходе - событие для подтверждения/отката транзакции - FinanceCreateESCommit
    */

    private void ConfigurePreCommitState()
    {
        During(PreCommitState,
        When(CommitEvent)
            .Publish(context => new AuctionDeleteESCommit
            {
                CorrelationId = context.Saga.CorrelationId
            })
        .TransitionTo(CommitState),
        When(FaultCommitEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new AuctionDeleteESCommit
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionMessage = context.Message.Message.ErrorExceptionMessage,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Message.Message.UserLogin
            })
        .TransitionTo(CommitState),
        //обработка ошибок - передаем отмену коммита и инфу по ошибке пользователю в UI
        When(FaultEvent)
            .Publish(context => new AuctionDeleteESCommit
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.ErrorMessage,
                ErrorExceptionMessage = context.Message.ErrorExceptionMessage,
                ErrorServiceName = context.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
        .TransitionTo(CommitState)
        );
    }

    private void ConfigureCommitState()
    {
        During(CommitState,
        When(CommitEvent)
            //посылаем через Кафку в EventSourcingService - для подтверждения транзакции
            .Activity(p => p.OfType<CommitActivity>())
            .TransitionTo(CompleteState));
    }

    private void ConfigureCompleteState()
    {
        During(CompleteState,
        When(NotificationUIEvent)
            //ждем сообщений об обработке всех 8 коллекций с записями
            .Then(context =>
            {
                lock (locker)
                {
                    context.Saga.CommitCounter--;
                }
            })
            .If(context => context.Saga.CommitCounter == 0,
            r => r
                .IfElse(context => context.Saga.IsError,
                p => p
                //в процессе выполнения произошла ошибка
                .Send(
                    new Uri(configuration["QueuePaths:ErrorNotificationConsumer"]),
                    context => new NotificationServiceError
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        ErrorMessage = context.Message.ErrorMessage,
                        ErrorExceptionMessage = context.Message.ErrorExceptionMessage,
                        ErrorServiceName = context.Message.ErrorServiceName,
                        UserLogin = context.Saga.UserLogin,
                        TraceId = Guid.NewGuid(),
                        IsError = context.Saga.IsError
                    }).Finalize(),
                p => p
                //Создаем событие в сервис NotificationService для обновления интерфейса
                .Send(
                    new Uri(configuration["QueuePaths:EventNotificationConsumer"]),
                    context => new EventNotificationItem
                    {
                        SignalRMethod = SignalRMethod.AuctionDelete,
                        AuctionId = context.Saga.ItemId,
                        Show = !context.Saga.IsError,
                        EventType = EventType.AuctionGroup_UserLogin,
                        UserLogin = context.Saga.UserLogin,
                        Data = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList)
                        .DataObjects.FirstOrDefault(p => p.DataType == "AuctionItem").Data
                    })).Finalize()
            ),
        //обрабатываем ошибки подтверждения/отката транзакции            
        When(FaultNotificationUIEvent)
        .Send(
            new Uri(configuration["QueuePaths:ErrorNotificationConsumer"]),
            context => new NotificationServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionMessage = context.Message.Message.ErrorExceptionMessage,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin,
                TraceId = Guid.NewGuid(),
                AuctionId = context.Saga.ItemId,
                IsError = context.Saga.IsError
            })
            .Finalize()
        );
    }
}