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
    public State NotificationState { get; }
    public State CommitState { get; }
    public State CompletedState { get; }

    public Event<RequestAuctionUpdate> RequestEvent { get; }
    public Event<ESLogAuctionUpdated> ImageEvent { get; }
    public Event<AuctionUpdateFinalize> ImageFinalizeEvent { get; }
    public Event<AuctionUpdatedGateWay> GatewayEvent { get; }
    public Event<AuctionUpdatedSearch> SearchEvent { get; }
    public Event<AuctionUpdatedElk> ElkEvent { get; }
    public Event<AuctionUpdatedNotification> NotificationEvent { get; }
    public Event<AuctionUpdateESCommit> CommitEvent { get; }
    public Event<AuctionUpdateComplete> CompleteEvent { get; }
    private IConfiguration configuration { get; }

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
        ConfigureNotificationState();
        ConfigureCommitState();
        ConfigureCompletedState();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestEvent, p => p.InsertOnInitial = true);
        Event(() => ImageEvent);
        Event(() => ImageFinalizeEvent);
        Event(() => GatewayEvent);
        Event(() => SearchEvent);
        Event(() => ElkEvent);
        Event(() => NotificationEvent);
        Event(() => CommitEvent);
        Event(() => CompleteEvent);
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
            })
            //посылаем через Кафку, выполнение всех операций в ES лог для обновления аукциона:
            // - Обновление записи в сервисе SearchService
            .Activity(p => p.OfType<ESLogActivity>())
            .TransitionTo(ImageState)
        );
        OnUnhandledEvent(async e => await e.Ignore());
        SetCompletedWhenFinalized();
    }

    private void ConfigureImageState()
    {
        During(ImageState,
        When(ImageEvent)
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

            //Обновление изображения аукциона в сервисе (если было изображение)            
            .If(context => context.Message.DataItems.DataObjects.Any(p => p.DataType == "ImageItem"),
                p => p
                .Send(
                    new Uri(configuration["QueuePaths:ImageConsumer"]),
                    context => new DataForProcessingServicesList<ImageDTO>
                    {
                        DataObjects = new List<DataForProcessingService>
                        {
                        //передаем изображение (или его часть)
                        string.IsNullOrEmpty(context.Saga.Image) ? new DataForProcessingService() :
                            JsonSerializer.Deserialize<DataForProcessingService>(context.Saga.Image),
                        //передаем тип операции
                            new DataForProcessingService
                            {
                                CRUD = context.Message.DataItems.DataObjects.Where(p => p.DataType == "ImageItem").First().CRUD,
                                Data = "CRUD",
                                MessagePartId = context.Saga.AuctionId
                            }
                        },
                        CorrelationId = context.Saga.CorrelationId,
                        CallBackType = "Common.Contracts.Auction.AuctionUpdatedGateWay"
                    })
            )

            //если было редактирование только текста аукциона, изображение осталось без изменений
            .If(context => string.IsNullOrEmpty(context.Saga.Image),
                p => p
                .Publish(context => new AuctionUpdatedGateWay
                {
                    CorrelationId = context.Message.CorrelationId
                })
            )

            //посылаем часть изображения для сохранения в ImageService
            .If(context => !context.Message.DataItems.DataObjects.Any() && context.Saga.UsingImage,
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
                                CRUD = CRUD.Update,
                                Data = "CRUD",
                                MessagePartId = context.Saga.AuctionId
                            }
                        },
                        CorrelationId = context.Saga.CorrelationId,
                        CallBackType = "Common.Contracts.Auction.AuctionUpdatedGateWay"
                    })
            )
            .TransitionTo(GatewayState)
        );
    }

    private void ConfigureGatewayState()
    {
        //получили обновленную запись аукциона
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
                    CallBackType = "Common.Contracts.Auction.AuctionUpdatedSearch"
                }),
            p => p
            .Publish(context => new AuctionUpdatedSearch
            {
                CorrelationId = context.Message.CorrelationId
            })
            )
            .TransitionTo(SearchState),
        //финализируем поток с частью изображения - чтобы пропал из лога
        When(ImageFinalizeEvent)
            .Finalize()
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
            .TransitionTo(ElkState));
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
                    CallBackType = "Common.Contracts.Auction.AuctionUpdatedNotification"
                })
            .TransitionTo(NotificationState));
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
                    DataObjects = new List<DataForProcessingService>(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionUpdateESCommit",
                    Props = JsonSerializer.Serialize(new AuctionNotificationData
                    {
                        AuctionData = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.FirstOrDefault(p => p.DataType == "AuctionItem").Data,
                        CorrelationId = context.Saga.CorrelationId,
                        CRUD = CRUD.Update
                    })
                })
            .TransitionTo(CommitState));
    }

    private void ConfigureCommitState()
    {
        During(CommitState,
        When(CommitEvent)
            //посылаем через Кафку в EventSourcingService - для подтверждения транзакции
            .Activity(p => p.OfType<CommitActivity>())
            .TransitionTo(CompletedState));
    }

    private void ConfigureCompletedState()
    {
        During(CompletedState,
        When(CompleteEvent).Finalize()
        );
    }

}