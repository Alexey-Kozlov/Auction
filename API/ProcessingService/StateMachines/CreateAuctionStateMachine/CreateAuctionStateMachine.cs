using System.Text.Json;
using Common.Contracts.Auction;
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
    public State CommitState { get; }
    public State CompletedState { get; }
    public State EndState { get; }


    public Event<RequestAuctionCreate> RequestEvent { get; }
    public Event<ESLog_AuctionCreated> EsLogEvent { get; }
    public Event<AuctionCreatedImage> ImageEvent { get; }
    public Event<AuctionCreatedSearch> SearchEvent { get; }
    public Event<AuctionCreatedElk> ElkEvent { get; }
    public Event<AuctionCreatedNotification> NotificationEvent { get; }
    public Event<AuctionCreateESCommit> CommitEvent { get; }
    public Event<AuctionCreateComplete> CompleteEvent { get; }

    private string Image { get; set; }
    private DataForProcessingServicesList ListItems { get; set; }
    private Guid InstanceCorrelationId { get; set; }
    private IConfiguration configuration { get; }

    public CreateAuctionStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureImageState();
        ConfigureSearchState();
        ConfigureNotificationState();
        ConfigureElkState();
        ConfigureCommitState();
        ConfigureCompletedState();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestEvent, p =>
        {
            p.InsertOnInitial = true;
        });
        Event(() => EsLogEvent);
        Event(() => ImageEvent);
        Event(() => SearchEvent, x => x.CorrelateById(p => InstanceCorrelationId));
        Event(() => ElkEvent);
        Event(() => NotificationEvent);
        Event(() => CommitEvent);
        Event(() => CompleteEvent);
    }
    private void ConfigureInitialState()
    {
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
                context.Saga.ReservePrice = context.Message.ReservePrice;
                Image = context.Message.Image;
                InstanceCorrelationId = context.Message.CorrelationId;
            })
            //посылаем через Кафку, выполнение всех операций в ES лог для создания аукциона:
            // - Создание записи в сервисе SearchService
            // - Создание записи в сервисе NotificationService
            .Activity(p => p.OfType<ESLogActivity>())
            .TransitionTo(ImageState)
        );
    }

    private void ConfigureImageState()
    {
        During(ImageState,
        When(EsLogEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
                ListItems = context.Message.DataItems;
                if (!string.IsNullOrEmpty(Image))
                {
                    var auction_str = ListItems.DataObjects.Where(p => p.DataType == "AuctionItem").ToList()[0].Data;
                    var auction_item = JsonSerializer.Deserialize<AuctionItem>(auction_str);
                    auction_item.Winner = Image;
                    auction_str = JsonSerializer.Serialize(auction_item);
                    ListItems.DataObjects.Where(p => p.DataType == "AuctionItem").ToList()[0].Data = auction_str;
                }

            })
            //Создание изображения аукциона в сервисе  (если было сделано)
            //Важно! Для передачи изображения в сервис ImageService используем неиспользуемое в этом
            //контексте текстовое поле Winner. Если создать поле Image для объекта AuctionItem - 
            //придется добавлять это поле в таблицу БД AuctionItem, что лишнее для этого одного функционала
            .IfElse(context => !string.IsNullOrEmpty(Image),
                p => p
                .Send(
                    new Uri(configuration["QueuePaths:ImageConsumer"]),
                    context => new DataForProcessingServicesList<AuctionItem>
                    {
                        DataObjects = ListItems.DataObjects.Where(p => p.DataType == "AuctionItem").ToList(),
                        CorrelationId = context.Saga.CorrelationId,
                        CallBackType = "Common.Contracts.Auction.AuctionCreatedSearch"
                    }),
                p => p
                .Publish(new AuctionCreatedSearch
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
                //удаляем изображение, если было. Оно уже не нужно
                if (!string.IsNullOrEmpty(Image))
                {
                    var auction_str = ListItems.DataObjects.Where(p => p.DataType == "AuctionItem").ToList()[0].Data;
                    var auction_item = JsonSerializer.Deserialize<AuctionItem>(auction_str);
                    auction_item.Winner = "";
                    auction_str = JsonSerializer.Serialize(auction_item);
                    ListItems.DataObjects.Where(p => p.DataType == "AuctionItem").ToList()[0].Data = auction_str;
                }
            })
            //Создание аукциона в сервисе SearchService
            .Send(
                new Uri(configuration["QueuePaths:SearchConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = ListItems.DataObjects.Where(p => p.DataType == "AuctionItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionCreatedNotification"
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
                    DataObjects = ListItems.DataObjects.Where(p => p.DataType == "NotifyItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionCreatedElk",
                    Props = context.Saga.Title
                })
            .TransitionTo(ElkState));
    }

    private void ConfigureElkState()
    {
        During(ElkState,
        When(ElkEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //посылаем в EldsticSearchService - для создания новой записи в индексе поиска
            .Send(
                new Uri(configuration["QueuePaths:ElkConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = ListItems.DataObjects.Where(p => p.DataType == "AuctionItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionCreateESCommit"
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