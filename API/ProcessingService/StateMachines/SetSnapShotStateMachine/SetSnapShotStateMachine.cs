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
    private object locker = new();

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
        Event(() => RequestEvent, p => p.InsertOnInitial = true);
        Event(() => ImageEvent);
        Event(() => BidEvent);
        Event(() => FinanceEvent);
        Event(() => SearchEvent);
        Event(() => NotifyEvent);
        Event(() => CompleteEvent);
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestEvent)
            .Then(context =>
            {
                context.Saga.CorrelationId = context.Message.CorrelationId;
                context.Saga.UserLogin = context.Message.UserLogin;
                context.Saga.NotifyMessage = "Создание SnapShot - ";
                context.Saga.BatchCounter = -1;
                context.Saga.AllItemsCount = 0;
                context.Saga.ProgressCurrent = 0; //Текущей прогресс в процентах
                context.Saga.SessionId = context.Message.SessionId;
                context.Saga.ActionDate = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:SetSnapShotProgressNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = context.Saga.SessionId,
                    Props = (context.Saga.ProgressCurrent = 5).ToString() + ";false"
                })
        //запрос батчей изображений
            .Send(
                new Uri(configuration["QueuePaths:ImageSetSnapShotConsumer"]),
                context => new ESContract
                {
                    CallBackType = "Common.Contracts.EventSourcing.ImageSetSnapShot",
                    CorrelationId = context.Message.CorrelationId
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
            // обрабатываем поступающие изображения (или части изображений)
            .If(context => context.Saga.BatchCounter == -1,
                p => p
                .Then(context =>
                {
                    //отрабатывает один раз - пишем общее количество записываемых в лог изображений
                    lock (locker)
                    {
                        context.Saga.BatchCounter = context.Message.AllItemsCount;
                        context.Saga.AllItemsCount = context.Message.AllItemsCount;
                        context.Saga.NotifyMessage += $" Изображений - {context.Message.AllItemsCount}";
                    }
                })
            )
            //каждое принятое изображение - пересылаем в EventSourcingService для сохранения в логе
            .If(context => context.Message.AllItemsCount > 0,
                p => p
                //посылаем на запись изображения или его части в ES лог
                .Send(
                    new Uri(configuration["QueuePaths:SetSnapShotConsumer"]),
                        context => new DataForProcessingServicesList<string>
                        {
                            DataObjects = context.Message.DataItems.DataObjects,
                            CorrelationId = context.Saga.CorrelationId,
                            CallBackType = "Common.Contracts.EventSourcing.ImageSetSnapShot",
                            Props = context.Saga.ActionDate.ToString()
                        }
                )
            )


            //пришел ответ после добавления изображения или его части в ES лог
            //AllItemsCount == -1 - признак что это ответ от EventSourcingService, полное изображение
            .If(context => context.Message.AllItemsCount == -1,
            p => p
            .Then(context =>
            {
                lock (locker)
                {
                    context.Saga.BatchCounter--;
                    context.Saga.ProgressCurrent = 5 + ((context.Saga.AllItemsCount - context.Saga.BatchCounter) * 85 / context.Saga.AllItemsCount);
                }
            }))

            //пришел ответ после добавления изображения или его части в ES лог
            //AllItemsCount == -2 - признак что это ответ от EventSourcingService, часть изображения изображение
            .If(context => context.Message.AllItemsCount == -2,
            p => p
            .Then(context =>
            {
                lock (locker)
                {
                    context.Saga.ProgressCurrent += float.Parse("0.1");
                }
            }))

            //обновляем показатель прогресса
            .Send(
                new Uri(configuration["QueuePaths:SetSnapShotProgressNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = context.Saga.SessionId,
                    Props = context.Saga.ProgressCurrent.ToString() + ";false"
                })

            //закончили прием изображений, переходим на обработку ставок
            .If(context => context.Saga.BatchCounter == 0,
            p => p
            .Publish(context => new BidSetSnapShot
            {
                CorrelationId = context.Message.CorrelationId
            })
            .TransitionTo(BidState))
        );
    }

    private void ConfigureBidState()
    {
        During(BidState,
        When(BidEvent)
            .Send(
                new Uri(configuration["QueuePaths:SetSnapShotProgressNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = context.Saga.SessionId,
                    Props = (context.Saga.ProgressCurrent += 2).ToString() + ";false"
                })
            //получение ставок (если есть) из сервиса BiddingService
            .Send(
                new Uri(configuration["QueuePaths:BidSetSnapShotConsumer"]),
                context => new ESContract
                {
                    CallBackType = "Common.Contracts.EventSourcing.FinanceSetSnapShot",
                    EventData = context.Saga.ActionDate.ToString(),
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
                context.Saga.NotifyMessage += $", Ставок - {context.Message.DataItems.DataObjects.Count()}";
            })
            .Send(
                new Uri(configuration["QueuePaths:SetSnapShotProgressNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = context.Saga.SessionId,
                    Props = (context.Saga.ProgressCurrent += 2).ToString() + ";false"
                })
            //получение записей финансов (если есть) из сервиса FinanceService
            .Send(
                new Uri(configuration["QueuePaths:FinanceSetSnapShotConsumer"]),
                context => new ESContract
                {
                    CallBackType = "Common.Contracts.EventSourcing.SearchSetSnapShot",
                    EventData = context.Saga.ActionDate.ToString(),
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
                context.Saga.NotifyMessage += $", Платежей - {context.Message.DataItems.DataObjects.Count()}";
            })
            .Send(
                new Uri(configuration["QueuePaths:SetSnapShotProgressNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = context.Saga.SessionId,
                    Props = (context.Saga.ProgressCurrent += 2).ToString() + ";false"
                })
            //получение записей аукционов из сервиса SearchService
            .Send(
                new Uri(configuration["QueuePaths:SearchSetSnapShotConsumer"]),
                context => new ESContract
                {
                    CallBackType = "Common.Contracts.EventSourcing.NotifySetSnapShot",
                    EventData = context.Saga.ActionDate.ToString(),
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
                context.Saga.NotifyMessage += $", Уведомлений - {context.Message.DataItems.DataObjects.Count()}";
            })
            .Send(
                new Uri(configuration["QueuePaths:SetSnapShotProgressNotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>(),
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = context.Saga.SessionId,
                    Props = (context.Saga.ProgressCurrent = 100).ToString() + ";true"
                })
            //Обновление записей уведомлений (если есть) в сервисе NotifyService
            .Send(
                new Uri(configuration["QueuePaths:NotifySetSnapShotConsumer"]),
                context => new ESContract
                {
                    CallBackType = "Common.Contracts.EventSourcing.SetSnapShotComplete",
                    EventData = context.Saga.ActionDate.ToString(),
                    CorrelationId = context.Saga.CorrelationId
                })
            .TransitionTo(CompletedState));
    }

    private void ConfigureCompletedState()
    {
        During(CompletedState,
        When(CompleteEvent)
            .Send(
                new Uri(configuration["QueuePaths:SetSnapShotFinalConsumer"]),
                context => new ESContract
                {
                    CallBackType = "",
                    EventData = context.Saga.NotifyMessage,
                    UserLogin = context.Saga.SessionId
                })
            .Finalize()
        );
    }



}