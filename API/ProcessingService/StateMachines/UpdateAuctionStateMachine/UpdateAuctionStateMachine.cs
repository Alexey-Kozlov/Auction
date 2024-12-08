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
    public State GatewayState { get; }
    public State ImageState { get; }
    public State SearchState { get; }
    public State ElkState { get; }
    public State NotificationState { get; }
    public State CommitState { get; }
    public State CompletedState { get; }

    public Event<RequestAuctionUpdate> RequestEvent { get; }
    public Event<ESLog_AuctionUpdated> EsLogEvent { get; }
    public Event<AuctionUpdatedImage> ImageEvent { get; }
    public Event<AuctionUpdatedSearch> SearchEvent { get; }
    public Event<AuctionUpdatedElk> ElkEvent { get; }
    public Event<AuctionUpdatedNotification> NotificationEvent { get; }
    public Event<AuctionUpdateESCommit> CommitEvent { get; }
    public Event<AuctionUpdateComplete> CompleteEvent { get; }
    private IConfiguration configuration { get; }
    private DataForProcessingServicesList ListItems { get; set; }
    private Guid InstanceCorrelationId { get; set; }

    public UpdateAuctionStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureGatewayState();
        ConfigureImageState();
        ConfigureSearchState();
        ConfigureNotificationState();
        ConfigureELKState();
        ConfigureCommitState();
        ConfigureCompletedState();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestEvent, p =>
        {
            p.InsertOnInitial = true;
        });
        Event(() => EsLogEvent, x => x.CorrelateById(p => InstanceCorrelationId));
        Event(() => ImageEvent, x => x.CorrelateById(p => InstanceCorrelationId));
        Event(() => SearchEvent, x => x.CorrelateById(p => InstanceCorrelationId));
        Event(() => ElkEvent, x => x.CorrelateById(p => InstanceCorrelationId));
        Event(() => NotificationEvent, x => x.CorrelateById(p => InstanceCorrelationId));
        Event(() => CommitEvent, x => x.CorrelateById(p => InstanceCorrelationId));
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
                context.Saga.CorrelationId = context.Message.CorrelationId;
                context.Saga.LastUpdated = DateTime.UtcNow;
                InstanceCorrelationId = context.Message.CorrelationId;
            })
            //посылаем через Кафку, выполнение всех операций в ES лог для обновления аукциона:
            // - Обновление записи в сервисе SearchService
            .Activity(p => p.OfType<ESLogActivity>())
            .TransitionTo(GatewayState)
        );
        SetCompletedWhenFinalized();
    }


    private void ConfigureGatewayState()
    {
        //получили обновленную запись аукциона
        During(GatewayState,
        When(EsLogEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
                ListItems = context.Message.DataItems;
            })
            //Удаление аукционв из кеша в сервисе GatewayService
            .Send(
                new Uri(configuration["QueuePaths:GatewayConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = context.Message.DataItems.DataObjects.Where(p => p.DataType == "AuctionItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionUpdatedImage"
                })
            .TransitionTo(ImageState));
    }
    private void ConfigureImageState()
    {
        During(ImageState,
        When(ImageEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //Обновление изображения аукциона в сервисе  (если было изображение)
            .IfElse(context => ListItems.DataObjects.Any(p => p.DataType == "ImageItem"),
                p => p
                .Send(
                    new Uri(configuration["QueuePaths:ImageConsumer"]),
                    context => new DataForProcessingServicesList<ImageDTO>
                    {
                        DataObjects = ListItems.DataObjects.Where(p => p.DataType == "ImageItem").ToList(),
                        CorrelationId = context.Saga.CorrelationId,
                        CallBackType = "Common.Contracts.Auction.AuctionUpdatedSearch"
                    }),
                p => p
                .Publish(new AuctionUpdatedSearch
                {
                    CorrelationId = InstanceCorrelationId
                })
            )
            .TransitionTo(SearchState));
    }
    private void ConfigureSearchState()
    {
        During(SearchState,
        When(SearchEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //Обновление аукциона в сервисе SearchService
            .Send(
                new Uri(configuration["QueuePaths:SearchConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = ListItems.DataObjects.Where(p => p.DataType == "AuctionItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionUpdatedElk"
                })
            .TransitionTo(ElkState));
    }

    private void ConfigureELKState()
    {
        During(ElkState,
        When(ElkEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //Обновление аукциона в поиске в сервисе ElasticSearchService
            .Send(
                new Uri(configuration["QueuePaths:ElkConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = ListItems.DataObjects.Where(p => p.DataType == "AuctionItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionUpdatedNotification"
                })
            .TransitionTo(NotificationState));
    }
    private void ConfigureNotificationState()
    {
        During(NotificationState,
        When(NotificationEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //Создание уведомления в сервисе NotificationService
            .Send(
                new Uri(configuration["QueuePaths:NotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionUpdateESCommit",
                    Props = JsonSerializer.Serialize(new AuctionNotificationData
                    {
                        AuctionData = ListItems.DataObjects.FirstOrDefault(p => p.DataType == "AuctionItem").Data,
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
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //посылаем через Кафку в EventSourcingService - для подтверждения транзакции
            .Activity(p => p.OfType<CommitActivity>())
            .TransitionTo(CompletedState));
    }

    private void ConfigureCompletedState()
    {
        During(CompletedState,
        When(CompleteEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Finalize()
        );
    }

}