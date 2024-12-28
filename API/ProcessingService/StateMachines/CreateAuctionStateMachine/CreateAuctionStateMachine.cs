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
    public State CommitState { get; }
    public State CompletedState { get; }
    public State EndState { get; }


    public Event<RequestAuctionCreate> RequestEvent { get; }
    public Event<ESLog_AuctionCreated> EsLogEvent { get; }
    public Event<AuctionUpdateFinalize> ImageFinalizeEvent { get; }
    public Event<AuctionCreatedSearch> SearchEvent { get; }
    public Event<AuctionCreatedElk> ElkEvent { get; }
    public Event<AuctionCreatedNotification> NotificationEvent { get; }
    public Event<AuctionCreateESCommit> CommitEvent { get; }
    public Event<AuctionCreateComplete> CompleteEvent { get; }

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
        Event(() => ImageFinalizeEvent);
        Event(() => SearchEvent);
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
                context.Saga.ReservePrice = context.Message.ReservePrice;
                context.Saga.Image = context.Message.Image;
                context.Saga.IsImageSplitted = context.Message.IsImageSplitted;
                context.Saga.UsingImage = context.Message.UsingImage;
            })
            //посылаем через Кафку, выполнение всех операций в ES лог для создания аукциона:
            // - Создание записи в сервисе SearchService
            // - Создание записи в сервисе NotificationService
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
                                CRUD = CRUD.Create,
                                Data = "CRUD",
                                MessagePartId = context.Saga.AuctionId
                            }
                        },
                        CorrelationId = context.Saga.CorrelationId,
                        CallBackType = "Common.Contracts.Auction.AuctionCreatedSearch"
                    })
            )

            //если было редактирование только текста аукциона, изображение осталось без изменений
            .If(context => string.IsNullOrEmpty(context.Saga.Image),
                p => p
                .Publish(context => new AuctionCreatedSearch
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
                                CRUD = CRUD.Create,
                                Data = "CRUD",
                                MessagePartId = context.Saga.AuctionId
                            }
                        },
                        CorrelationId = context.Saga.CorrelationId,
                        CallBackType = "Common.Contracts.Auction.AuctionCreatedSearch"
                    })
            )
            .TransitionTo(SearchState));
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
        //финализируем поток с частью изображения - чтобы пропал из лога
        When(ImageFinalizeEvent)
            .Finalize()
        );
    }

    private void ConfigureNotificationState()
    {
        During(NotificationState,
        When(NotificationEvent)
            //Создание уведомления в сервисе NotificationService
            .Send(
                new Uri(configuration["QueuePaths:NotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "NotifyItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionCreatedElk",
                    Props = JsonSerializer.Serialize(new AuctionNotificationData
                    {
                        AuctionData = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.FirstOrDefault(p => p.DataType == "AuctionItem").Data,
                        CorrelationId = context.Saga.CorrelationId,
                        CRUD = CRUD.Create
                    })
                })
            .TransitionTo(ElkState));
    }

    private void ConfigureElkState()
    {
        During(ElkState,
        When(ElkEvent)
            //посылаем в EldsticSearchService - для создания новой записи в индексе поиска
            .Send(
                new Uri(configuration["QueuePaths:ElkConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects.Where(p => p.DataType == "AuctionItem").ToList(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionCreateESCommit"
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