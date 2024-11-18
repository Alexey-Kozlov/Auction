using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.ELKSearch;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using MassTransit;
using ProcessingService.Activities.ElkIndex;

namespace ProcessingService.StateMachines.ElkIndexStateMachine;
public class ElkIndexStateMachine : MassTransitStateMachine<ElkIndexState>
{
    public State ResetIndexState { get; }
    public State ElkIndexState { get; }
    public State NotificationState { get; }
    public State CompletedState { get; }
    public State FaultedState { get; }

    public Event<ElkIndexReset> ResetIndexEvent { get; }
    public Event<RequestElkIndex> RequestElkIndexEvent { get; }
    public Event<ESLog_ElkIndex> EsLogEvent { get; }
    public Event<ElkIndexCompleted> NotificationEvent { get; }
    public Event<ElkIndexEnd> EndEvent { get; }
    private IConfiguration configuration { get; }
    private DataForProcessingServicesList ListItems { get; set; }
    private ElkIndexResponse elkIndexResponse { get; set; }
    private Guid InstanceCorrelationId { get; set; }
    private int CurrentBatchCount { get; set; }
    private int AllBatchCount { get; set; }
    private object locker = new();

    public ElkIndexStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureResetIndexState();
        ConfigureElkIndexState();
        ConfigureNotificationState();
        ConfigureCompleted();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestElkIndexEvent, p =>
        {
            p.InsertOnInitial = true;
        });
        Event(() => ResetIndexEvent);
        Event(() => EsLogEvent);
        Event(() => NotificationEvent, x => x.CorrelateById(p => InstanceCorrelationId));
        Event(() => EndEvent);
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestElkIndexEvent)
            .Then(context =>
            {
                context.Saga.CorrelationId = context.Message.CorrelationId;
                context.Saga.SessionId = context.Message.SessionId;
                context.Saga.UserLogin = context.Message.UserLogin;
                InstanceCorrelationId = context.Message.CorrelationId;
                CurrentBatchCount = 0;
                AllBatchCount = 0;
            })
            //посылаем сообщение для сброса индекса поиска
            .Send(
                new Uri(configuration["QueuePaths:ElkConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = new List<DataForProcessingService>
                    {
                        new DataForProcessingService
                        {
                            CRUD = CRUD.Create,
                            DataType = "ElkIndexReset",
                            Data = JsonSerializer.Serialize(new AuctionItem())
                        }
                    },
                    CorrelationId = context.Message.CorrelationId,
                    CallBackType = "Common.Contracts.ELKSearch.ElkIndexReset"
                })

            .TransitionTo(ResetIndexState)
        );
        OnUnhandledEvent(async e => await e.Ignore());
    }
    private void ConfigureResetIndexState()
    {
        During(ResetIndexState,
        When(ResetIndexEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //посылаем через Кафку, запрос на индексацию всех записей аукционов:
            .Activity(p => p.OfType<ESLogActivity>())
            .TransitionTo(ElkIndexState)
        );
    }

    private void ConfigureElkIndexState()
    {

        During(ElkIndexState,
        When(EsLogEvent)
            .Then(context =>
            {
                //получили список аукционов для индексации
                context.Saga.LastUpdated = DateTime.UtcNow;
                if (context.Message.DataItems != null)
                {
                    ListItems = context.Message.DataItems;
                    AllBatchCount = context.Message.BatchCount;
                    context.Saga.ItemNumber = context.Message.AllItemsCount;
                }
                else
                {
                    lock (locker)
                    {
                        CurrentBatchCount++;
                    }
                }
            })

            .If(context => context.Message.DataItems != null,
                p => p
                .Send(
                new Uri(configuration["QueuePaths:ElkConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = ListItems.DataObjects,
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Processing.ESLog_ElkIndex"
                })
            )
            .If(context => AllBatchCount == CurrentBatchCount,
                p => p
                .Then(context =>
                {
                    Console.WriteLine("Всего - " + context.Saga.ItemNumber + " записи индексировано.");
                })
                .Publish(new ElkIndexCompleted
                {
                    CorrelationId = InstanceCorrelationId
                })
                .TransitionTo(NotificationState)
            )
        );
    }
    private void ConfigureNotificationState()
    {
        During(NotificationState,
        When(NotificationEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
                //инициализируем объект для уведомления о результатах индексации
                elkIndexResponse = new ElkIndexResponse
                {
                    CorrelationId = context.Saga.CorrelationId,
                    ItemNumber = context.Saga.ItemNumber,
                    SessionId = context.Saga.SessionId
                };
            })
            //Создание уведомления в сервисе NotificationService
            .Send(
                new Uri(configuration["QueuePaths:NotificationConsumer"]),
                context => new DataForProcessingServicesList<NotifyItem>
                {
                    DataObjects = new List<DataForProcessingService>
                    {
                        new DataForProcessingService
                        {
                            CRUD = CRUD.Create,
                            DataType = "ElkIndex",
                            Data = JsonSerializer.Serialize(elkIndexResponse,elkIndexResponse.GetType())
                        }
                    },
                    CorrelationId = elkIndexResponse.CorrelationId,
                    CallBackType = "Common.Contracts.ELKSearch.ElkIndexEnd"
                })
            .TransitionTo(CompletedState)
        );
    }

    private void ConfigureCompleted()
    {
        During(CompletedState,
            When(EndEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            }).Finalize()
        );
    }

}