using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Image;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using MassTransit;
using ProcessingService.Activities.AuctionCreate;

namespace ProcessingService.StateMachines.CreateAuctionStateMachine;

public class CreateAuctionStateMachine : MassTransitStateMachine<CreateAuctionState>
{
    public State ImageState { get; }
    public State SearchState { get; }
    public State NotificationState { get; }
    public State ElkState { get; }
    public State PreCommitState { get; }
    public State CommitState { get; }
    public State CompleteState { get; }
    public State AbortState { get; }


    public Event<RequestAuctionCreate> RequestEvent { get; }
    public Event<ESLogAuctionCreated> EsLogEvent { get; }
    public Event<AuctionCreateFinalize> ImageFinalizeEvent { get; }
    public Event<AuctionCreatedSearch> SearchEvent { get; }
    public Event<AuctionCreatedElk> ElkEvent { get; }
    public Event<AuctionCreatedNotification> NotificationEvent { get; }
    public Event<AuctionCreatedNotificationEvent> NotificationUIEvent { get; }
    public Event<AuctionCreateESCommit> CommitEvent { get; }
    public Event<BaseServiceError> FaultEvent { get; }
    public Event<Fault<AuctionCreateESCommit>> FaultCommitEvent { get; }
    public Event<Fault<ESLogAuctionCreated>> FaultEsLogEvent { get; }
    public Event<Fault<AuctionCreatedSearch>> FaultSearchEvent { get; }
    public Event<Fault<AuctionCreatedElk>> FaultElkEvent { get; }
    public Event<Fault<AuctionCreatedNotification>> FaultNotificationEvent { get; }
    public Event<Fault<AuctionCreatedNotificationEvent>> FaultNotificationUIEvent { get; }
    public object locker = new();
    private IConfiguration configuration { get; }

    public CreateAuctionStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureImageState();
        ConfigureSearchState();
        ConfigureElkState();
        ConfigurePreCommitState();
        ConfigureCommitState();
        ConfigureNotificationState();
        ConfigureCompleteState();
        ConfigureAbortState();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestEvent, p =>
        {
            p.InsertOnInitial = true;
        });
        Event(() => EsLogEvent);
        Event(() => ImageFinalizeEvent);
        Event(() => SearchEvent);
        Event(() => ElkEvent);
        Event(() => NotificationEvent);
        Event(() => CommitEvent);
        Event(() => FaultEvent);
        Event(() => FaultEsLogEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultSearchEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultElkEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultNotificationEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultCommitEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultNotificationUIEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestEvent)
            .Then(context =>
            {
                context.Saga.ItemId = context.Message.ItemId;
                context.Saga.Title = context.Message.Title;
                context.Saga.Description = context.Message.Description;
                context.Saga.Properties = context.Message.Properties;
                context.Saga.UserLogin = context.Message.UserLogin;
                context.Saga.AuctionEnd = context.Message.AuctionEnd;
                context.Saga.ReservePrice = context.Message.ReservePrice;
                context.Saga.Image = context.Message.Image;
                context.Saga.IsImageSplitted = context.Message.IsImageSplitted;
                context.Saga.UsingImage = context.Message.UsingImage;
                context.Saga.IsError = false;
                context.Saga.CommitCounter = 5;
            })
            //посылаем через Кафку, выполнение всех операций в ES лог для создания аукциона
            .Activity(p => p.OfType<ESLogActivity>())
            .TransitionTo(ImageState)
        );
        SetCompletedWhenFinalized();
    }

    private void ConfigureImageState()
    {
        During(ImageState,
        When(EsLogEvent)
            //вернулся ответ от записи изображения в ES лог
            //каждый ответ пересылаем в ImageService
            .If(context => context.Message.DataItems.DataObjects.Any(),
                //здесь ответ о завершении передачи полного изображения, или при его отсутствии            
                p => p
                .Then(context =>
                {
                    context.Saga.DataForProcessingServicesList = JsonSerializer.Serialize(context.Message.DataItems);
                })
            )

            //если было редактирование только текста аукциона, изображение осталось без изменений
            .If(context => string.IsNullOrEmpty(
                    JsonSerializer.Deserialize<DataForProcessingService>(context.Saga.Image).Data),
                p => p
                .Publish(context => new AuctionCreatedSearch
                {
                    CorrelationId = context.Message.CorrelationId
                })
            )

            //посылаем часть изображения для сохранения в ImageService
            .If(context => !string.IsNullOrEmpty(JsonSerializer.Deserialize<DataForProcessingService>(context.Saga.Image).Data)
                 && context.Saga.UsingImage,
            p => p
              .Send(
                    new Uri(configuration["QueuePaths:ImageConsumer"]),
                    context => new DataForProcessingServicesList<ImageDTO>
                    {
                        DataObjects = new List<DataForProcessingService>
                        {
                        //передаем изображение (или его часть)
                        JsonSerializer.Deserialize<DataForProcessingService>(context.Saga.Image),
                        //передаем тип операции
                            new DataForProcessingService
                            {
                                CRUD = CRUD.Create,
                                Data = "CRUD",
                                MessagePartId = context.Saga.ItemId.Value,
                                DataType = context.Saga.UserLogin //костыль - передаем UserLogin через это неиспользуемое поле
                            }
                        },
                        CorrelationId = context.Saga.CorrelationId,
                        CallBackType = "Common.Contracts.Auction.AuctionCreatedSearch,Common.Contracts.Auction.AuctionCreateFinalize"
                    })
            )
            .TransitionTo(SearchState),
        //обрабатываем ошибки из сервиса EventSourcingService            
        When(FaultEsLogEvent)
            .Then(p => p.Saga.IsError = true)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionStack = context.Message.Message.ErrorExceptionStack,
                ErrorExceptionInputData = context.Message.Message.ErrorExceptionInputData,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
            .TransitionTo(AbortState)
        );
    }

    private void ConfigureSearchState()
    {
        During(SearchState,
        When(SearchEvent)
            //Создание аукциона в сервисе SearchService
            .Send(
                new Uri(configuration["QueuePaths:SearchConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "AuctionItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionCreatedNotification"
                })
            .TransitionTo(NotificationState),
        //финализируем поток с частью изображения - чтобы завершить процесс
        When(ImageFinalizeEvent)
            .Finalize(),
        //обрабатываем ошибки из сервиса ImageService            
        When(FaultSearchEvent)
            .Then(p => p.Saga.IsError = true)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionStack = context.Message.Message.ErrorExceptionStack,
                ErrorExceptionInputData = context.Message.Message.ErrorExceptionInputData,
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
            //Создание уведомления в сервисе NotificationService
            .Send(
                new Uri(configuration["QueuePaths:AuctionEditConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList)
                        .DataObjects.Where(p => p.DataType == "NotifyItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionCreatedElk",
                    Props = ""
                })
            .TransitionTo(ElkState),
        //обрабатываем ошибки из сервиса SearchService            
        When(FaultNotificationEvent)
            .Then(p => p.Saga.IsError = true)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionStack = context.Message.Message.ErrorExceptionStack,
                ErrorExceptionInputData = context.Message.Message.ErrorExceptionInputData,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
        .TransitionTo(PreCommitState)
        );
    }

    private void ConfigureElkState()
    {
        During(ElkState,
        When(ElkEvent)
            //посылаем в ElasticSearchService - для создания новой записи в индексе поиска
            .Send(
                new Uri(configuration["QueuePaths:ElkConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "AuctionItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionCreateESCommit"
                })
            .TransitionTo(PreCommitState),
        //обрабатываем ошибки из сервиса NotificationService            
        When(FaultElkEvent)
            .Then(p => p.Saga.IsError = true)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionStack = context.Message.Message.ErrorExceptionStack,
                ErrorExceptionInputData = context.Message.Message.ErrorExceptionInputData,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin
            })
            .TransitionTo(PreCommitState)
        );
    }

    /*промежуточный этап перед подтверждением/откатом транзакции
    на входе события:
    - BaseServiceError - событие ошибок от предыдущих этапов
    - Fault<AuctionCreateESCommit> - событие ошибки предыдущего этапа
    - AuctionCreateESCommit - событие правильного выполнения предыдущего этапа
    на выходе - событие для подтверждения/отката транзакции - FinanceCreateESCommit
    */

    private void ConfigurePreCommitState()
    {
        During(PreCommitState,
        When(CommitEvent)
            .Publish(context => new AuctionCreateESCommit
            {
                CorrelationId = context.Saga.CorrelationId
            })
        .TransitionTo(CommitState),
        When(FaultCommitEvent)
            .Then(p => p.Saga.IsError = true)
            .Publish(context => new AuctionCreateESCommit
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.Message.ErrorMessage,
                ErrorExceptionStack = context.Message.Message.ErrorExceptionStack,
                ErrorExceptionInputData = context.Message.Message.ErrorExceptionInputData,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Message.Message.UserLogin
            })
        .TransitionTo(CommitState),
        //обработка ошибок - передаем отмену коммита и инфу по ошибке пользователю в UI
        When(FaultEvent)
            .Publish(context => new AuctionCreateESCommit
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.ErrorMessage,
                ErrorExceptionStack = context.Message.ErrorExceptionStack,
                ErrorExceptionInputData = context.Message.ErrorExceptionInputData,
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
            //посылаем через Кафку в EventSourcingService - для подтверждения/отмены транзакции
            .Activity(p => p.OfType<CommitActivity>())
            .TransitionTo(CompleteState)
        );
    }

    private void ConfigureCompleteState()
    {
        During(CompleteState,
        When(NotificationUIEvent)
            //ждем сообщений об обработке всех 5 коллекций с записями
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
                        ErrorExceptionStack = context.Message.ErrorExceptionStack,
                        ErrorExceptionInputData = context.Message.ErrorExceptionInputData,
                        ErrorServiceName = context.Message.ErrorServiceName,
                        UserLogin = context.Saga.UserLogin,
                        TraceId = Guid.NewGuid(),
                    }).Finalize(),
                p => p
                //Создаем событие в сервис NotificationService для обновления интерфейса
                .Send(
                    new Uri(configuration["QueuePaths:EventNotificationConsumer"]),
                    context => new EventNotificationItem
                    {
                        SignalRMethod = SignalRMethod.AuctionCreate,
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
                ErrorExceptionStack = context.Message.Message.ErrorExceptionStack,
                ErrorExceptionInputData = context.Message.Message.ErrorExceptionInputData,
                ErrorServiceName = context.Message.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin,
                TraceId = Guid.NewGuid(),
                AuctionId = context.Saga.ItemId,
            })
        .Finalize()
        );
    }

    private void ConfigureAbortState()
    {
        During(AbortState,
        When(FaultEvent)
        .Send(
            new Uri(configuration["QueuePaths:ErrorNotificationConsumer"]),
            context => new NotificationServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                ErrorMessage = context.Message.ErrorMessage,
                ErrorExceptionStack = context.Message.ErrorExceptionStack,
                ErrorExceptionInputData = context.Message.ErrorExceptionInputData,
                ErrorServiceName = context.Message.ErrorServiceName,
                UserLogin = context.Saga.UserLogin,
                TraceId = Guid.NewGuid(),
            })
        .Finalize());
    }
}