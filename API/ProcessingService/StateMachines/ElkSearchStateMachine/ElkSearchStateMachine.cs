using System.Text.Json;
using Common.Contracts;
using Common.Contracts.Auction;
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
    public Event<ElkSearchCreated<ApiResponse<PagedResult<List<AuctionCreatingElk>>>>> ElkSearchEvent { get; }
    public Event<ElkSearchResponseCompleted> CompleteEvent { get; }
    private IConfiguration configuration { get; }

    public ElkSearchStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureCompletedState();
        ConfigureNotificationState();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestElkSearchEvent, p => p.InsertOnInitial = true);
        Event(() => ElkSearchEvent);
        Event(() => CompleteEvent);
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestElkSearchEvent)
            .Then(context =>
            {
                context.Saga.AuctionId = context.Message.Id;
                context.Saga.Term = context.Message.SearchTerm;
                context.Saga.PageSize = context.Message.PageSize;
                context.Saga.PageNumber = context.Message.PageNumber;
                context.Saga.CorrelationId = context.Message.CorrelationId;
                context.Saga.SessionId = context.Message.SessionId;
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
                                context.Message.Id,
                                context.Saga.CorrelationId,
                                context.Message.SearchTerm,
                                context.Message.PageNumber,
                                context.Message.PageSize
                            ))
                        }
                    },
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.ELKSearch.ElkSearchCreated<ApiResponse<PagedResult<List<AuctionCreatingElk>>>>"
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
                new Uri(configuration["QueuePaths:ElkSearchNotificationConsumer"]),
                context => new DataForProcessingServicesList<ApiResponse<PagedResult<List<AuctionCreatingElk>>>>
                {
                    DataObjects = new List<DataForProcessingService>
                    {
                        new DataForProcessingService
                        {
                            CRUD = CRUD.Create,
                            DataType = "ElkSearch",
                            Data = JsonSerializer.Serialize(context.Message.Result)
                        }
                    },
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.ELKSearch.ElkSearchResponseCompleted",
                    Props = context.Saga.SessionId
                })
            .TransitionTo(NotificationState));
    }
    private void ConfigureCompletedState()
    {
        During(NotificationState,
        When(CompleteEvent).Finalize());
    }
}