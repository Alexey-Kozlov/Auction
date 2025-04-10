using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Image;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using MassTransit;
using ProcessingService.Activities.AuctionUpdate;

namespace ProcessingService.StateMachines.UpdateAuctionStateMachine;
public class UpdateAuctionStateMachine : MassTransitStateMachine<UpdateAuctionState>
{
    public State ImageState { get; }
    public State GatewayState { get; }
    public State SearchState { get; }
    public State ElkState { get; }
    public State PreCommitState { get; }
    public State CommitState { get; }
    public State CompleteState { get; }

    public Event<RequestAuctionUpdate> RequestEvent { get; }
    public Event<ESLogAuctionUpdated> EsLogEvent { get; }
    public Event<AuctionUpdateFinalize> ImageFinalizeEvent { get; }
    public Event<AuctionUpdatedGateWay> GatewayEvent { get; }
    public Event<AuctionUpdatedSearch> SearchEvent { get; }
    public Event<AuctionUpdatedElk> ElkEvent { get; }
    public Event<AuctionUpdatedNotificationEvent> NotificationUIEvent { get; }
    public Event<AuctionUpdateESCommit> CommitEvent { get; }
    public Event<BaseServiceError> FaultEvent { get; }
    public Event<Fault<ESLogAuctionUpdated>> FaultEsLogEvent { get; }
    public Event<Fault<AuctionUpdatedGateWay>> FaultGatewayEvent { get; }
    public Event<Fault<AuctionUpdatedSearch>> FaultSearchEvent { get; }
    public Event<Fault<AuctionUpdatedElk>> FaultElkEvent { get; }
    public Event<Fault<AuctionUpdateESCommit>> FaultCommitEvent { get; }
    public Event<Fault<AuctionUpdatedNotificationEvent>> FaultNotificationUIEvent { get; }
    private IConfiguration configuration { get; }
    public object locker = new();

    public UpdateAuctionStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureImageState();
        ConfigureGatewayState();
        ConfigureSearchState();
        ConfigureELKState();
        ConfigurePreCommitState();
        ConfigureCommitState();
        ConfigureCompleteState();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestEvent, p => p.InsertOnInitial = true);
        Event(() => EsLogEvent);
        Event(() => ImageFinalizeEvent);
        Event(() => GatewayEvent);
        Event(() => SearchEvent);
        Event(() => ElkEvent);
        Event(() => NotificationUIEvent);
        Event(() => CommitEvent);
        Event(() => FaultEvent);
        Event(() => FaultEsLogEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultGatewayEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultSearchEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultElkEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultCommitEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultNotificationUIEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
    }
    private void ConfigureInitialState()
    {
        //инициализация
        Initially(
            When(RequestEvent)
            .Then(context =>
            {
                context.Saga.AuctionId = context.Message.AuctionId;
                context.Saga.Title = context.Message.Title;
                context.Saga.Description = context.Message.Description;
                context.Saga.Properties = context.Message.Properties;
                context.Saga.UserLogin = context.Message.UserLogin;
                context.Saga.AuctionEnd = context.Message.AuctionEnd;
                context.Saga.Image = context.Message.Image;
                context.Saga.IsImageSplitted = context.Message.IsImageSplitted;
                context.Saga.UsingImage = context.Message.UsingImage;
                context.Saga.IsError = false;
                context.Saga.CommitCounter = 5;
            })
            //посылаем через Кафку, выполнение всех операций в ES лог для обновления аукциона:
            // - Обновление записи в сервисе SearchService
            .Activity(p => p.OfType<ESLogActivity>())
            .TransitionTo(ImageState)
        );
        //OnUnhandledEvent(async e => await e.Ignore());
        SetCompletedWhenFinalized();
    }

    private void ConfigureImageState()
    {
        During(ImageState,
        When(EsLogEvent)
            //вернулся ответ от записи изображения в ES лог
            //каждый ответ пересылаем в ImageService
            .If(context => context.Message.DataItems.DataObjects.Any(),
                //сохраняем первоначально переданный набор сообщений        
                p => p
                .Then(context =>
                {
                    context.Saga.DataForProcessingServicesList = JsonSerializer.Serialize(context.Message.DataItems);
                })
            )

            //посылаем часть изображения для сохранения в ImageService в случаях:
            //если есть изображение и если указано не использовать изображение
            //в первом случае обновляем изображение, во втором - удаляем изображение
            .IfElse(context => !string.IsNullOrEmpty(JsonSerializer.Deserialize<DataForProcessingService>(context.Saga.Image).Data)
                 || !context.Saga.UsingImage,
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
                                CRUD = context.Saga.UsingImage ? CRUD.Update : CRUD.Delete,
                                Data = "CRUD",
                                MessagePartId = context.Saga.AuctionId
                            }
                        },
                        CorrelationId = context.Saga.CorrelationId,
                        CallBackType = "Common.Contracts.Auction.AuctionUpdatedSearch,Common.Contracts.Auction.AuctionUpdateFinalize"
                    }),
            //здесь если не трогали изображение - пропускаем функционал обработки изображений
            p => p
                .Publish(context => new AuctionUpdatedSearch
                {
                    CorrelationId = context.Message.CorrelationId
                })
            )
            .TransitionTo(SearchState),
        //обрабатываем ошибки из сервиса EventSourcingService            
        When(FaultEsLogEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                Message = context.Message.Message.Message,
                ExceptionMessage = context.Message.Message.ExceptionMessage,
                ServiceName = context.Message.Message.ServiceName,
                UserLogin = context.Saga.UserLogin
            })
        .TransitionTo(PreCommitState)
        );
    }

    private void ConfigureSearchState()
    {
        During(SearchState,
        When(SearchEvent)
            //Обновление аукциона в сервисе SearchService
            .Send(
                new Uri(configuration["QueuePaths:SearchConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "AuctionItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionUpdatedElk"
                })
            .TransitionTo(ElkState),
        //финализируем поток с частью изображения - чтобы пропал из лога
        When(ImageFinalizeEvent).Finalize(),
        //обрабатываем ошибки из сервиса GatewayService            
        When(FaultSearchEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                Message = context.Message.Message.Message,
                ExceptionMessage = context.Message.Message.ExceptionMessage,
                ServiceName = context.Message.Message.ServiceName,
                UserLogin = context.Saga.UserLogin
            })
        .TransitionTo(PreCommitState)
        );
    }

    private void ConfigureELKState()
    {
        During(ElkState,
        When(ElkEvent)
            //Обновление аукциона в поиске в сервисе ElasticSearchService
            .Send(
                new Uri(configuration["QueuePaths:ElkConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "AuctionItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionUpdatedGateWay"
                })
            .TransitionTo(GatewayState),
        //обрабатываем ошибки из сервиса SearchService            
        When(FaultElkEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                Message = context.Message.Message.Message,
                ExceptionMessage = context.Message.Message.ExceptionMessage,
                ServiceName = context.Message.Message.ServiceName,
                UserLogin = context.Saga.UserLogin
            })
        .TransitionTo(PreCommitState)
        );
    }

    private void ConfigureGatewayState()
    {
        During(GatewayState,
        When(GatewayEvent)
        //Удаление изображения из кеша в сервисе GatewayService (если были изменено изображение)
        .IfElse(context => JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "ImageItem").Any(),
            p => p
            .Send(
                new Uri(configuration["QueuePaths:GatewayConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "ImageItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionUpdateESCommit"
                }),
            p => p
            .Publish(context => new AuctionUpdateESCommit
            {
                CorrelationId = context.Message.CorrelationId
            })
            )
            .TransitionTo(PreCommitState),
        //обрабатываем ошибки из сервиса ImageService            
        When(FaultGatewayEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Publish(context => new BaseServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                Message = context.Message.Message.Message,
                ExceptionMessage = context.Message.Message.ExceptionMessage,
                ServiceName = context.Message.Message.ServiceName,
                UserLogin = context.Saga.UserLogin
            })
        .TransitionTo(PreCommitState)
        );
    }

    /*промежуточный этап перед подтверждением/откатом транзакции
       на входе события:
       - BaseServiceError - событие ошибок от предыдущих этапов
       - Fault<AuctionUpdateESCommit> - событие ошибки предыдущего этапа
       - AuctionUpdateESCommit - событие правильного выполнения предыдущего этапа
       на выходе - событие для подтверждения/отката транзакции - FinanceCreateESCommit
       */

    private void ConfigurePreCommitState()
    {
        During(PreCommitState,
        When(CommitEvent)
            .Publish(context => new AuctionUpdateESCommit
            {
                CorrelationId = context.Saga.CorrelationId
            })
        .TransitionTo(CommitState),
        When(FaultCommitEvent)
            .Then(p => p.Saga.IsError = p.Message.Message.IsError)
            .Send(
                new Uri(configuration["QueuePaths:ErrorNotificationConsumer"]),
                context => new NotificationServiceError
                {
                    CorrelationId = context.Saga.CorrelationId,
                    Message = context.Message.Message.Message,
                    ExceptionMessage = context.Message.Message.ExceptionMessage,
                    ServiceName = context.Message.Message.ServiceName,
                    UserLogin = context.Saga.UserLogin,
                    IsError = context.Message.Message.IsError
                })
            .Publish(context => new AuctionUpdateESCommit
            {
                CorrelationId = context.Saga.CorrelationId
            })
        .TransitionTo(CommitState),
        //обработка ошибок - передаем отмену коммита и инфу по ошибке пользователю в UI
        When(FaultEvent)
            .Send(
                new Uri(configuration["QueuePaths:ErrorNotificationConsumer"]),
                context => new NotificationServiceError
                {
                    CorrelationId = context.Saga.CorrelationId,
                    Message = context.Message.Message,
                    ExceptionMessage = context.Message.ExceptionMessage,
                    ServiceName = context.Message.ServiceName,
                    UserLogin = context.Saga.UserLogin,
                    TraceId = Guid.NewGuid(),
                    IsError = context.Saga.IsError
                })
            .Publish(context => new AuctionUpdateESCommit
            {
                CorrelationId = context.Saga.CorrelationId
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
                .IfElse(context => context.Saga.DataForProcessingServicesList == null,
                p => p
                //Создаем событие в сервис NotificationService для обновления интерфейса
                .Send(
                    new Uri(configuration["QueuePaths:AuctionEventConsumer"]),
                    context => new DataForProcessingServicesList<NotifyItem>
                    {
                        DataObjects = new List<DataForProcessingService>(),
                        CorrelationId = context.Saga.CorrelationId,
                        CallBackType = "",
                        Props = $"{!context.Saga.IsError}"
                    }),
                p => p
                //Создаем событие в сервис NotificationService для обновления интерфейса
                .Send(
                new Uri(configuration["QueuePaths:AuctionEventConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList)
                        .DataObjects.Where(p => p.DataType == "AuctionItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "",
                    Props = $"{!context.Saga.IsError}"
                })).Finalize()
            ),
        //обрабатываем ошибки подтверждения/отката транзакции            
        When(FaultNotificationUIEvent)
        .Send(
            new Uri(configuration["QueuePaths:ErrorNotificationConsumer"]),
            context => new NotificationServiceError
            {
                CorrelationId = context.Saga.CorrelationId,
                Message = context.Message.Message.Message,
                ExceptionMessage = context.Message.Message.ExceptionMessage,
                ServiceName = context.Message.Message.ServiceName,
                UserLogin = context.Saga.UserLogin,
                TraceId = Guid.NewGuid(),
                AuctionId = context.Saga.AuctionId,
                IsError = context.Saga.IsError
            })
            .Finalize()
        );
    }

}