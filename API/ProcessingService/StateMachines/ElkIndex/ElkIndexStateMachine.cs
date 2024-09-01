using Common.Contracts;
using MassTransit;

namespace ProcessingService.StateMachines.ElkIndexStateMachine;
public class ElkIndexStateMachine : MassTransitStateMachine<ElkIndexState>
{
    public State ElkIndexCreatedState { get; }
    public State ElkIndexNotificationState { get; }
    public State CompletedState { get; }
    public State FaultedState { get; }

    public Event<ElkIndexRequest> RequestElkIndexEvent { get; }
    public Event<ElkIndexCreated> ElkIndexCompletedEvent { get; }
    public Event<ElkIndexResponseCompleted> ElkIndexNotificationSendedEvent { get; }
    private IConfiguration configuration { get; }

    public ElkIndexStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureElkIndexCompleted();
        ConfigureElkIndexNotificationCompleted();
        ConfigureCompleted();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestElkIndexEvent);
        Event(() => ElkIndexCompletedEvent);
        Event(() => ElkIndexNotificationSendedEvent);
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestElkIndexEvent)
            .Then(context =>
            {
                context.Saga.Id = context.Message.Id;
                context.Saga.Title = context.Message.Item.Title;
                context.Saga.Description = context.Message.Item.Description;
                context.Saga.Properties = context.Message.Item.Properties;
                context.Saga.AuctionAuthor = context.Message.Item.AuctionAuthor;
                context.Saga.AuctionEnd = context.Message.Item.AuctionEnd;
                context.Saga.Amount = context.Message.Item.Amount;
                context.Saga.CorrelationId = context.Message.CorrelationId;
                context.Saga.LastUpdated = DateTime.UtcNow;
                context.Saga.ReservePrice = context.Message.Item.ReservePrice;
                context.Saga.ItemSold = context.Message.Item.ItemSold;
                context.Saga.Winner = context.Message.Item.Winner;
                context.Saga.LastItem = context.Message.LastItem;
                context.Saga.ItemNumber = context.Message.ItemNumber;
                context.Saga.SessionId = context.Message.SessionId;
                context.Saga.AuctionCreated = context.Message.Item.AuctionCreated;
            })
            .Send(
                new Uri(configuration["QueuePaths:ElkIndexCreating"]),
                context => new ElkIndexCreating(
                context.Saga.CorrelationId,
                context.Message.Item,
                context.Saga.ItemNumber
            ))
            .TransitionTo(ElkIndexCreatedState)
        );
    }

    private void ConfigureElkIndexCompleted()
    {
        During(ElkIndexCreatedState,
        When(ElkIndexCompletedEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:ElkIndexResponse"]),
                context => new ElkIndexResponse(
                context.Message.CorrelationId,
                context.Message.Result,
                context.Saga.LastItem,
                context.Saga.ItemNumber,
                context.Saga.SessionId
                ))
            .TransitionTo(ElkIndexNotificationState));
    }
    private void ConfigureElkIndexNotificationCompleted()
    {
        During(ElkIndexNotificationState,
        When(ElkIndexNotificationSendedEvent)
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