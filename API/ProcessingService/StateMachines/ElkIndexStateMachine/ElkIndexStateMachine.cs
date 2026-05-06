using System.Reflection;
using Common.Contracts.Auction;
using Common.Contracts.ELKSearch;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using MassTransit;
using ProcessingService.Activities.ElkIndex;

namespace ProcessingService.StateMachines.ElkIndexStateMachine;

public class ElkIndexStateMachine : MassTransitStateMachine<ElkIndexState>
{
    public State ResetIndexState { get; }
    public State ElkIndexState { get; }
    public State PreCommitState { get; }
    public State CommitState { get; }
    public State CompleteState { get; }


    public Event<ElkIndexRequest> RequestElkIndexEvent { get; }
    public Event<ElkIndexReset> ResetIndexEvent { get; }
    public Event<ESLogElkIndex> EsLogEvent { get; }
    public Event<ElkIndexCompleted> NotificationEvent { get; }
    public Event<ElkIndexESCommit> CommitEvent { get; }
    public Event<BaseServiceError> FaultEvent { get; }
    public Event<Fault<ElkIndexReset>> FaultResetIndexEvent { get; }
    public Event<Fault<ESLogElkIndex>> FaultEsLogEvent { get; }
    public Event<Fault<ElkIndexESCommit>> FaultCommitEvent { get; }
    public Event<Fault<ElkIndexCompleted>> FaultNotificationEvent { get; }
    private IConfiguration configuration { get; }

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
        ConfigurePreCommitState();
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
        Event(() => FaultEvent);
        Event(() => FaultResetIndexEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultEsLogEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultNotificationEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
        Event(() => FaultCommitEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestElkIndexEvent)
            .Then(context =>
            {
                context.Saga.UserLogin = context.Message.UserLogin;
                context.Saga.IsError = false;
                context.Saga.CallBackType = context.Message.CallBackType;
                context.Saga.ShowMessages = context.Message.ShowMessages;
                CurrentBatchCount = 0;
                AllBatchCount = 0;
            })
            //посылаем сообщение для сброса индекса поиска
            .Send(
                new Uri(configuration["QueuePaths:ElkReset"]),
                context => new ElkIndexResetRequest
                {
                    CorrelationId = context.Message.CorrelationId,
                    CallBackType = "Common.Contracts.ELKSearch.ElkIndexReset"
                })
            .TransitionTo(ResetIndexState)
        );

        //Далее - оператор для подавления ошибки - что сообщение не было принято и обработано
        //без этого оператора будут ошибки, т.к. у нас генерируется много сообщений в сервис ElasticSearchService
        //и принимаются оттуда же без передачи в конкретное состояние.

        //OnUnhandledEvent(async e => await e.Ignore());

        SetCompletedWhenFinalized();
    }
    private void ConfigureResetIndexState()
    {
        During(ResetIndexState,
        When(ResetIndexEvent)
            //посылаем через Кафку, запрос на индексацию всех записей аукционов 
            // + записей сообщений пользователей + теги.
            //возвращаются пачки записей для переиндексации из ES лога
            .Activity(p => p.OfType<ESLogActivity>())
            .TransitionTo(ElkIndexState),
        //обрабатываем ошибки из сервиса ElasticSearchService            
        When(FaultResetIndexEvent)
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
            //означает, что все пакеты из ES лог обработаны и можно заканчивать индексацию
            .If(context => AllBatchCount == CurrentBatchCount,
                p => p
                .Publish(context => new ElkIndexESCommit
                {
                    CorrelationId = context.Message.CorrelationId
                })
                .TransitionTo(PreCommitState)
            ),
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
            .TransitionTo(PreCommitState)
        );
    }

    /*промежуточный этап перед подтверждением/откатом транзакции
   на входе события:
   - BaseServiceError - событие ошибок от предыдущих этапов
   - Fault<ElkIndexESCommit> - событие ошибки предыдущего этапа
   - ElkIndexESCommit - событие правильного выполнения предыдущего этапа
   на выходе - событие для подтверждения/отката транзакции - ElkIndexESCommit
   */

    private void ConfigurePreCommitState()
    {
        During(PreCommitState,
        When(CommitEvent)
            .Publish(context => new ElkIndexESCommit
            {
                CorrelationId = context.Saga.CorrelationId
            })
        .TransitionTo(CommitState),
        When(FaultCommitEvent)
            .Then(p => p.Saga.IsError = true)
            .Publish(context => new ElkIndexESCommit
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
            .Publish(context => new ElkIndexESCommit
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
            //посылаем через Кафку в EventSourcingService - для подтверждения транзакции
            .Activity(p => p.OfType<CommitActivity>())
            .TransitionTo(CompleteState));
    }

    private void ConfigureCompleted()
    {
        During(CompleteState,
        When(NotificationEvent)
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
                        SignalRMethod = SignalRMethod.ElkIndexReset,
                        Show = context.Saga.ShowMessages,
                        EventType = EventType.UserLogin,
                        UserLogin = context.Saga.UserLogin,
                        Data = $"Проиндексировано - {context.Saga.ItemNumber} записей"
                    })
                // если в начальном сообщении был указан параметр CallBackType - посылаем сообщение
                // по этому параметру - это значит был вызов процесса индексации из другого процесса
                .If(context => !string.IsNullOrEmpty(context.Saga.CallBackType),
                    P => P
                    .Publish(t =>
                    {
                        var messageObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                            configuration["CommonAssembly"]).CreateInstance(t.Saga.CallBackType);
                        messageObject.GetType().GetProperty("CorrelationId").SetValue(messageObject, t.Saga.CorrelationId);
                        //для отсылки необходим явный тип сообщения, иначе будет ошибка - Messages types must not be in the System namespace
                        switch (t.Saga.CallBackType)
                        {
                            case "Common.Contracts.EventSourcing.RestoreSnapShotESCommit":
                                return (RestoreSnapShotESCommit)messageObject;
                        }
                        return null;
                    })))
        .Finalize(),
        //обрабатываем ошибки подтверждения/отката транзакции            
        When(FaultNotificationEvent)
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
                })
        .Finalize()
        );
    }

}