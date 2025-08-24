using System.Text.Json;
using Common.Contracts.ELKSearch;
using Common.Contracts.Processing;
using MassTransit;

namespace ProcessingService.StateMachines.ElkSearchStateMachine;

public class ElkSearchStateMachine : MassTransitStateMachine<ElkSearchState>
{
    public State ElkSearchState { get; }
    public State NotificationState { get; }
    public State CompletedState { get; }

    public Event<ElkSearchRequest> RequestElkSearchEvent { get; }
    public Event<ElkSearchResult> ElkSearchEvent { get; }
    public Event<Fault<ElkSearchResult>> FaultElkSearchEvent { get; }
    private IConfiguration configuration { get; }

    public ElkSearchStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureNotificationState();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestElkSearchEvent, p => p.InsertOnInitial = true);
        Event(() => ElkSearchEvent);
        Event(() => FaultElkSearchEvent, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestElkSearchEvent)
            .Then(context =>
            {
                context.Saga.AuctionId = context.Message.ItemId;
                context.Saga.Term = context.Message.SearchTerm;
                context.Saga.PageSize = context.Message.PageSize;
                context.Saga.PageNumber = context.Message.PageNumber;
                context.Saga.IsError = false;
            })
            .Send(
                new Uri(configuration["QueuePaths:ElkSearchCreating"]),
                context => new DataForProcessingServicesList<ElkSearchCreating>
                {
                    DataObjects = new List<DataForProcessingService>
                    {
                        new DataForProcessingService
                        {
                            CRUD = CRUD.Create,
                            DataType = "ElkSearchCreating",
                            Data = JsonSerializer.Serialize(new ElkSearchCreating(
                                context.Message.ItemId,
                                context.Saga.CorrelationId,
                                context.Message.SearchTerm,
                                context.Message.PageNumber,
                                context.Message.PageSize
                            ))
                        }
                    },
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.ELKSearch.ElkSearchResult"
                })
            .TransitionTo(NotificationState)
        );
        SetCompletedWhenFinalized();
    }

    private void ConfigureNotificationState()
    {
        During(NotificationState,
        When(ElkSearchEvent)
            .Send(
                new Uri(configuration["QueuePaths:EventNotificationConsumer"]),
                    context => new EventNotificationItem
                    {
                        SignalRMethod = SignalRMethod.ElkSearch,
                        Show = true,
                        UserLogin = context.Saga.UserLogin,
                        EventType = EventType.UserLogin,
                        Data = JsonSerializer.Serialize(context.Message.Result)
                    }).Finalize(),
        //обрабатываем ошибки из сервиса ElasticSearchService            
        When(FaultElkSearchEvent)
            .Send(
                new Uri(configuration["QueuePaths:ErrorNotificationConsumer"]),
                context => new NotificationServiceError
                {
                    CorrelationId = context.Saga.CorrelationId,
                    ErrorMessage = context.Message.Message.ErrorMessage,
                    ErrorExceptionMessage = context.Message.Message.ErrorExceptionMessage,
                    ErrorServiceName = context.Message.Message.ErrorServiceName,
                    UserLogin = context.Saga.UserLogin,
                    TraceId = Guid.NewGuid(),
                    IsError = true
                })
            .Finalize());
    }
}