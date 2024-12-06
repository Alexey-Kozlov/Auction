using Common.Contracts.EventSourcing;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using MassTransit;

namespace ProcessingService.StateMachines.SetSnapShotStateMachine;
public class SetSnapShotStateMachine : MassTransitStateMachine<SetSnapShotState>
{
    public State ImagesState { get; }
    public State BidState { get; }
    public State FinanceState { get; }
    public State SearchState { get; }
    public State NotifyState { get; }
    public State CompletedState { get; }


    public Event<RequestSetSnapShot> RequestEvent { get; }
    public Event<ImageSetSnapShot> ImageEvent { get; }
    public Event<BidSetSnapShot> BidEvent { get; }
    public Event<FinanceSetSnapShot> FinanceEvent { get; }
    public Event<SearchSetSnapShot> SearchEvent { get; }
    public Event<NotifySetSnapShot> NotifyEvent { get; }
    public Event<SetSnapShotComplete> CompleteEvent { get; }

    private IConfiguration configuration { get; }
    private Guid InstanceCorrelationId { get; set; }
    private string NotifyMessage { get; set; }
    private int BatchCounter { get; set; }
    private float AllItemsCount { get; set; }
    private object locker = new();
    private float ProgressCurrent { get; set; }
    private string SessionId { get; set; }
    private DateTime ActionDate { get; set; }

    public SetSnapShotStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureImagesState();
        ConfigureBidState();
        ConfigureFinanceState();
        ConfigureSearchState();
        ConfigureNotifyState();
        ConfigureCompletedState();

    }
    private void ConfigureEvents()
    {
        Event(() => RequestEvent, p =>
        {
            p.InsertOnInitial = true;
        });
        Event(() => ImageEvent, x => x.CorrelateById(p => InstanceCorrelationId));
        Event(() => BidEvent, x => x.CorrelateById(p => InstanceCorrelationId));
        Event(() => FinanceEvent, x => x.CorrelateById(p => InstanceCorrelationId));
        Event(() => SearchEvent, x => x.CorrelateById(p => InstanceCorrelationId));
        Event(() => NotifyEvent, x => x.CorrelateById(p => InstanceCorrelationId));
        Event(() => CompleteEvent, x => x.CorrelateById(p => InstanceCorrelationId));
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestEvent)
            .Then(context =>
            {
                context.Saga.CorrelationId = context.Message.CorrelationId;
                context.Saga.LastUpdated = DateTime.UtcNow;
                context.Saga.UserLogin = context.Message.UserLogin;
                NotifyMessage = "Создание SnapShot - ";
                InstanceCorrelationId = context.Message.CorrelationId;
                BatchCounter = -1;
                ProgressCurrent = 0; //Текущей прогресс в процентах
                SessionId = context.Message.SessionId;
                ActionDate = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:SetSnapShotProgressNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = SessionId,
                    Props = (ProgressCurrent = 5).ToString()
                })
        //запрос батчей изображений
            .Send(
                new Uri(configuration["QueuePaths:ImageSetSnapShotConsumer"]),
                context => new ESContract
                {
                    CallBackType = "Common.Contracts.EventSourcing.ImageSetSnapShot"
                })
            .TransitionTo(ImagesState)
        );
        OnUnhandledEvent(async e => await e.Ignore());
        SetCompletedWhenFinalized();
    }

    private void ConfigureImagesState()
    {
        During(ImagesState,
        When(ImageEvent)
            // обрабатываем поступающие наборы изображений
            .If(context => context.Message.DataItems.DataObjects.Count() > 0 && context.Message.AllItemsCount != -1,
            p => p
            .Then(context =>
            {
                if (BatchCounter == -1)
                {
                    BatchCounter = context.Message.AllItemsCount;
                    AllItemsCount = context.Message.AllItemsCount;
                    NotifyMessage += $" Изображений - {context.Message.AllItemsCount}";
                }
            })
            //посылаем на запись батча изображений в ES лог
            .Send(
                new Uri(configuration["QueuePaths:SetSnapShotConsumer"]),
                    context => new DataForProcessingServicesList<string>
                    {
                        DataObjects = context.Message.DataItems.DataObjects,
                        CorrelationId = context.Saga.CorrelationId,
                        CallBackType = "Common.Contracts.EventSourcing.ImageSetSnapShot",
                        Props = ActionDate.ToString()
                    })
            )
            //пришел ответ после добавления батча в ES лог
            .If(context => context.Message.AllItemsCount == -1,
            p => p
            //закончили прием батчей изображений, переходим на прием сообщений об обработке изображений
            .Then(context =>
            {
                lock (locker)
                {
                    BatchCounter -= context.Message.DataItems.DataObjects.Count();
                    ProgressCurrent += context.Message.DataItems.DataObjects.Count() * 75 / AllItemsCount;
                }

            }))
            .Send(
                new Uri(configuration["QueuePaths:SetSnapShotProgressNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = SessionId,
                    Props = ProgressCurrent.ToString()
                })
            //пришел ответ после добавления батча в ES лог
            .If(context => BatchCounter == 0,
            p => p
            //закончили прием батчей изображений, переходим на обработку ставок
            .Publish(new BidSetSnapShot
            {
                CorrelationId = InstanceCorrelationId
            })
            .TransitionTo(BidState))
        );
    }

    private void ConfigureBidState()
    {
        During(BidState,
        When(BidEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:SetSnapShotProgressNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = SessionId,
                    Props = (ProgressCurrent += 5).ToString()
                })
            //получение ставок (если есть) из сервиса BiddingService
            .Send(
                new Uri(configuration["QueuePaths:BidSetSnapShotConsumer"]),
                context => new ESContract
                {
                    CallBackType = "Common.Contracts.EventSourcing.FinanceSetSnapShot",
                    EventData = ActionDate.ToString(),
                    CorrelationId = context.Saga.CorrelationId
                })
            .TransitionTo(FinanceState));
    }

    private void ConfigureFinanceState()
    {
        During(FinanceState,
        When(FinanceEvent)
           .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
                NotifyMessage += $", Ставок - {context.Message.DataItems.DataObjects.Count()}";
            })
            .Send(
                new Uri(configuration["QueuePaths:SetSnapShotProgressNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = SessionId,
                    Props = (ProgressCurrent += 5).ToString()
                })
            //получение записей финансов (если есть) из сервиса FinanceService
            .Send(
                new Uri(configuration["QueuePaths:FinanceSetSnapShotConsumer"]),
                context => new ESContract
                {
                    CallBackType = "Common.Contracts.EventSourcing.SearchSetSnapShot",
                    EventData = ActionDate.ToString(),
                    CorrelationId = context.Saga.CorrelationId
                })
            .TransitionTo(SearchState));
    }

    private void ConfigureSearchState()
    {
        During(SearchState,
        When(SearchEvent)
           .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
                NotifyMessage += $", Платежей - {context.Message.DataItems.DataObjects.Count()}";
            })
            .Send(
                new Uri(configuration["QueuePaths:SetSnapShotProgressNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = SessionId,
                    Props = (ProgressCurrent += 5).ToString()
                })
            //получение записей аукционов из сервиса SearchService
            .Send(
                new Uri(configuration["QueuePaths:SearchSetSnapShotConsumer"]),
                context => new ESContract
                {
                    CallBackType = "Common.Contracts.EventSourcing.NotifySetSnapShot",
                    EventData = ActionDate.ToString(),
                    CorrelationId = context.Saga.CorrelationId
                })
            .TransitionTo(NotifyState));
    }

    private void ConfigureNotifyState()
    {
        During(NotifyState,
        When(NotifyEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
                NotifyMessage += $", Уведомлений - {context.Message.DataItems.DataObjects.Count()}";
            })
            .Send(
                new Uri(configuration["QueuePaths:SetSnapShotProgressNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = SessionId,
                    Props = (ProgressCurrent = 100).ToString()
                })
            //Обновление записей уведомлений (если есть) в сервисе NotifyService
            .Send(
                new Uri(configuration["QueuePaths:NotifySetSnapShotConsumer"]),
                context => new ESContract
                {
                    CallBackType = "Common.Contracts.EventSourcing.SetSnapShotComplete",
                    EventData = ActionDate.ToString(),
                    CorrelationId = context.Saga.CorrelationId
                })
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
            .Send(
                new Uri(configuration["QueuePaths:SetSnapShotFinalConsumer"]),
                context => new ESContract
                {
                    CallBackType = "",
                    EventData = NotifyMessage,
                    UserLogin = SessionId
                })
            .Finalize()
        );
    }



}