using Common.Contracts;
using MassTransit;
using ProcessingService.Activities.AuctionDelete;
using ProcessingService.Activities.AuctionFinish;

namespace ProcessingService.StateMachines.FinishAuctionStateMachine;
public class FinishAuctionStateMachine : MassTransitStateMachine<FinishAuctionState>
{
    public State AuctionFinishedState { get; }
    public State AuctionFinishedFinanceState { get; }
    public State AuctionFinishedNotificationState { get; }
    public State AuctionFinishedSearchState { get; }
    public State AuctionFinishedElkState { get; }
    public State CommitAuctionFinishedState { get; }
    public State CompletedState { get; }
    public State FaultedState { get; }

    public Event<RequestAuctionFinish> RequestAuctionFinishingEvent { get; }
    public Event<AuctionFinished> AuctionFinishedEvent { get; }
    public Event<AuctionFinishedFinance> AuctionFinishedFinanceEvent { get; }
    public Event<AuctionFinishedNotification> AuctionFinishedNotificationEvent { get; }
    public Event<AuctionFinishedSearch> AuctionFinishedSearchEvent { get; }
    public Event<AuctionFinishedElk> AuctionFinishedElkEvent { get; }
    public Event<CommitAuctionFinishedContract> CommitAuctionFinishedEvent { get; }
    public Event<GetAuctionFinishState> AuctionFinishedStateEvent { get; }
    private IConfiguration configuration { get; }

    public FinishAuctionStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureAuctionFinished();
        ConfigureAuctionFinishedFinance();
        ConfigureAuctionFinishedSearch();
        ConfigureAuctionFinishedNotification();
        ConfigureAuctionFinishedElk();
        ConfigureCommitFinishingAuction();
        ConfigureCompleted();
        ConfigureGetState();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestAuctionFinishingEvent, p =>
        {
            p.InsertOnInitial = true;
        });
        Event(() => AuctionFinishedEvent);
        Event(() => AuctionFinishedFinanceEvent);
        Event(() => AuctionFinishedNotificationEvent);
        Event(() => AuctionFinishedSearchEvent);
        Event(() => AuctionFinishedStateEvent);
        Event(() => AuctionFinishedElkEvent);
        Event(() => CommitAuctionFinishedEvent);
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestAuctionFinishingEvent)
            .Then(context =>
            {
                context.Saga.Id = context.Message.Id;
                context.Saga.Winner = context.Message.Winner;
                context.Saga.ItemSold = context.Message.ItemSold;
                context.Saga.Amount = context.Message.Amount;
                context.Saga.CorrelationId = context.Message.CorrelationId;
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Activity(p => p.OfType<FinishingAuctionActivity>())
            .TransitionTo(AuctionFinishedState)
        );
    }

    private void ConfigureAuctionFinished()
    {
        During(AuctionFinishedState,
        When(AuctionFinishedEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:AuctionFinishingFinance"]),
                context => new AuctionFinishingFinance(
                context.Saga.Id,
                context.Saga.ItemSold,
                context.Saga.Winner,
                context.Saga.CorrelationId))
            .TransitionTo(AuctionFinishedFinanceState));
    }
    private void ConfigureAuctionFinishedFinance()
    {
        During(AuctionFinishedFinanceState,
        When(AuctionFinishedFinanceEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:AuctionFinishingSearch"]),
                context => new AuctionFinishingSearch(
                context.Saga.Id,
                context.Saga.ItemSold,
                context.Saga.Winner,
                context.Saga.Amount,
                context.Saga.CorrelationId))
            .TransitionTo(AuctionFinishedSearchState));
    }

    private void ConfigureAuctionFinishedSearch()
    {
        During(AuctionFinishedSearchState,
        When(AuctionFinishedSearchEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:AuctionFinishingNotification"]),
                context => new AuctionFinishingNotification(
                context.Saga.Id,
                context.Saga.ItemSold,
                context.Saga.Winner,
                context.Saga.Amount,
                context.Saga.CorrelationId))
            .TransitionTo(AuctionFinishedNotificationState));
    }
    private void ConfigureAuctionFinishedNotification()
    {
        During(AuctionFinishedNotificationState,
        When(AuctionFinishedNotificationEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Send(
                new Uri(configuration["QueuePaths:AuctionFinishingElk"]),
                context => new AuctionFinishingElk(
                context.Saga.Id,
                context.Saga.ItemSold,
                context.Saga.Winner,
                context.Saga.Amount,
                context.Saga.CorrelationId))
            .TransitionTo(AuctionFinishedElkState));
    }

    private void ConfigureAuctionFinishedElk()
    {
        During(AuctionFinishedElkState,
        When(AuctionFinishedElkEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            .Activity(p => p.OfType<CommitFinishingAuctionActivity>())
            .TransitionTo(CommitAuctionFinishedState));
    }

    private void ConfigureCommitFinishingAuction()
    {
        During(CommitAuctionFinishedState,
        When(CommitAuctionFinishedEvent)
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


    private void ConfigureGetState()
    {
        DuringAny(
            When(AuctionFinishedStateEvent)
                .Respond(x => x.Saga)
        );
    }

}