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
    public State CommitState { get; }

    public Event<ElkIndexReset> ResetIndexEvent { get; }
    public Event<RequestElkIndex> RequestElkIndexEvent { get; }
    public Event<ESLogElkIndex> EsLogEvent { get; }
    public Event<ElkIndexCompleted> NotificationEvent { get; }
    public Event<ElkIndexEnd> EndEvent { get; }
    public Event<ElkIndexESCommit> CommitEvent { get; }
    private IConfiguration configuration { get; }
    private ElkIndexResponse elkIndexResponse { get; set; }
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
        ConfigureCommitState();
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
        Event(() => NotificationEvent);
        Event(() => CommitEvent);
        Event(() => EndEvent);
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestElkIndexEvent)
            .Then(context =>
            {
                context.Saga.SessionId = context.Message.SessionId;
                context.Saga.UserLogin = context.Message.UserLogin;
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

        //ВАЖНО! Этот оператор для подавления ошибки - что сообщение не было принято и обработано
        //без этого оператора будут ошибки, т.к. у нас генерируется много сообщений в сервис ElasticSearchService
        //и принимаются оттуда же без передачи в конкретное состояние.
        OnUnhandledEvent(async e => await e.Ignore());
        SetCompletedWhenFinalized();
    }
    private void ConfigureResetIndexState()
    {
        During(ResetIndexState,
        When(ResetIndexEvent)
            //посылаем через Кафку, запрос на индексацию всех записей аукционов.
            //возвращаются пачки записей для переиндексации из ES лога
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
                if (context.Message.DataItems != null)
                {
                    //если DataItems != null - это пришел набор записей из ES лог
                    //сохраняем:
                    //AllBatchCount - общее количество наборов записей
                    //ItemNumber - общее количество индексированных записей
                    AllBatchCount = context.Message.BatchCount;
                    context.Saga.ItemNumber = context.Message.AllItemsCount;
                }
                else
                {
                    //если DataItems == null - это пришел ответ из ElasticSearchService, после индексации
                    //посланного туда набора записей.Увеличиваем счетчик индексированных наборов записей.
                    lock (locker)
                    {
                        CurrentBatchCount++;
                    }
                }
            })
            //если DataItems != null - это пришел набор записей из ES лог, посылаем этот набор в ElasticSearchService
            //для индексации, обратно вернется сообщение СЮДА ЖЕ, в ЭТОТ ЖЕ МЕТОД! Отличительный признак сообщения
            //из ElasticSearchService - у него DataItems == null
            //Сколько сообщений пошлем в ElasticSearchService, столько же ответов сюда вернется.
            //Проверка на null - чтобы не было бесконечного цикла посылки и приема сообщений
            .If(context => context.Message.DataItems != null,
                p => p
                //посылаем сообщение в ElasticSearchService для идексации пакета записей из ES лог
                .Send(
                new Uri(configuration["QueuePaths:ElkConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = context.Message.DataItems.DataObjects,
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Processing.ESLogElkIndex"
                })
            )
            //Если переданное общее количество переданных пакетов соответствует счетчику обработанных пакетов -
            //означает, что все пакеты из ES лог обработаны и можно посылать уведомление об окончания индексации
            .If(context => AllBatchCount == CurrentBatchCount,
                p => p
                .Then(context =>
                {
                    Console.WriteLine("Всего - " + context.Saga.ItemNumber + " записи индексировано.");
                })
                .Publish(context => new ElkIndexCompleted
                {
                    CorrelationId = context.Message.CorrelationId
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
                //инициализируем объект для уведомления о результатах индексации
                elkIndexResponse = new ElkIndexResponse
                {
                    CorrelationId = context.Saga.CorrelationId,
                    ItemNumber = context.Saga.ItemNumber, //общее количнство индексированных записей
                    SessionId = context.Saga.SessionId
                };
            })
            //Создание уведомления в сервисе NotificationService
            .Send(
                new Uri(configuration["QueuePaths:ElkIndexNotificationConsumer"]),
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
                    CallBackType = "Common.Contracts.ELKSearch.ElkIndexESCommit"
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
            .TransitionTo(CompletedState));
    }

    private void ConfigureCompleted()
    {
        During(CompletedState,
            When(EndEvent).Finalize()
        );
    }

}