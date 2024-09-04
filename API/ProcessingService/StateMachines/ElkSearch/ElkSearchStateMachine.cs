using Common.Contracts;
using Common.Utils;
using MassTransit;

namespace ProcessingService.StateMachines.ElkSearchStateMachine;
public class ElkSearchStateMachine : MassTransitStateMachine<ElkSearchState>
{
    public State ElkSearchCreatedState { get; }
    public State ElkSearchNotificationState { get; }
    public State CompletedState { get; }
    public State FaultedState { get; }

    public Event<ElkSearchRequest> RequestElkSearchEvent { get; }
    public Event<ElkSearchCreated<ApiResponse<PagedResult<List<AuctionCreatingElk>>>>> ElkSearchCompletedEvent { get; }
    public Event<ElkSearchResponseCompleted> ElkSearchNotificationSendedEvent { get; }
    private IConfiguration configuration { get; }

    public ElkSearchStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureElkSearchCompleted();
        ConfigureElkSearchNotificationCompleted();
        ConfigureCompleted();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestElkSearchEvent);
        Event(() => ElkSearchCompletedEvent);
        Event(() => ElkSearchNotificationSendedEvent);
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestElkSearchEvent)
            .Then(context =>
            {
                context.Saga.Id = context.Message.Id;
                context.Saga.Term = context.Message.SearchTerm;
                context.Saga.PageSize = context.Message.PageSize;
                context.Saga.PageNumber = context.Message.PageNumber;
                context.Saga.CorrelationId = context.Message.CorrelationId;
                context.Saga.SessionId = context.Message.SessionId;
            })
            .Send(
                new Uri(configuration["QueuePaths:ElkSearchCreating"]),
                context => new ElkSearchCreating(
                context.Message.Id,
                context.Saga.CorrelationId,
                context.Message.SearchTerm,
                context.Message.PageNumber,
                context.Message.PageSize
            ))
            .TransitionTo(ElkSearchCreatedState)
        );
    }

    private void ConfigureElkSearchCompleted()
    {
        During(ElkSearchCreatedState,
        When(ElkSearchCompletedEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:ElkSearchResponse"]),
                context => new ElkSearchResponse<ApiResponse<PagedResult<List<AuctionCreatingElk>>>>(
                context.Message.CorrelationId,
                context.Saga.Term,
                context.Message.ResultType,
                context.Message.Result,
                context.Saga.SessionId
                ))
            .TransitionTo(ElkSearchNotificationState));
    }
    private void ConfigureElkSearchNotificationCompleted()
    {
        During(ElkSearchNotificationState,
        When(ElkSearchNotificationSendedEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .TransitionTo(CompletedState));
    }

    private void ConfigureCompleted()
    {
        During(CompletedState);
    }

}