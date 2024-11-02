using Common.Contracts;
using MassTransit;
using ProcessingService.Activities.AuctionDelete;
using ProcessingService.StateMachines.DeleteAuctionStateMachine;

namespace ProcessingService.StateMachines.UpdateAuctionStateMachine;
public class DeleteAuctionStateMachine : MassTransitStateMachine<DeleteAuctionState>
{
    public State FinanceState { get; }
    public State BidState { get; }
    public State GatewayState { get; }
    public State ImageState { get; }
    public State SearchState { get; }
    public State ElkState { get; }
    public State NotificationState { get; }
    public State CommitState { get; }
    public State CompletedState { get; }

    public Event<RequestAuctionDelete> RequestEvent { get; }
    public Event<AuctionDeletedFinance> FinanceEvent { get; }
    public Event<AuctionDeletedBid> BidEvent { get; }
    public Event<AuctionDeletedGateway> GatewayEvent { get; }
    public Event<AuctionDeletedImage> ImageEvent { get; }
    public Event<AuctionDeletedSearch> SearchEvent { get; }
    public Event<AuctionDeletedElk> ElkEvent { get; }
    public Event<AuctionDeletedNotification> NotificationEvent { get; }
    public Event<AuctionDeleteESCommit> CommitEvent { get; }
    private IConfiguration configuration { get; }

    public DeleteAuctionStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        // ConfigureAuctionDeletedFinance();
        // ConfigureAuctionDeletedBid();
        // ConfigureAuctionDeletedGateway();
        // ConfigureAuctionDeletedImage();
        // ConfigureAuctionDeletedSearch();
        // ConfigureAuctionDeletedNotification();
        // ConfigureAuctionDeletedElk();
        // ConfigureCommitDeletingAuction();
        // ConfigureCompleted();
        // ConfigureGetState();
    }
    private void ConfigureEvents()
    {
        Event(() => RequestEvent, p =>
        {
            p.InsertOnInitial = true;
        });
        Event(() => FinanceEvent);
        Event(() => BidEvent);
        Event(() => GatewayEvent);
        Event(() => ImageEvent);
        Event(() => SearchEvent);
        Event(() => ElkEvent);
        Event(() => NotificationEvent);
        Event(() => CommitEvent);
    }
    private void ConfigureInitialState()
    {
        Initially(
            When(RequestEvent)
            .Then(context =>
            {
                context.Saga.AuctionId = context.Message.AuctionId;
                context.Saga.UserLogin = context.Message.UserLogin;
                context.Saga.CorrelationId = context.Message.CorrelationId;
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //посылаем через Кафку в EventSourcingService -> CreateEventSourcingItemConsumer
            //Удаление записей по деньгам в сервисе FinanceService
            .Activity(p => p.OfType<FinanceActivity>())
            .TransitionTo(FinanceState)
        );
    }

    private void ConfigureFinanceState()
    {
        During(FinanceState,
        When(FinanceEvent)
            .Then(context =>
            {
                context.Saga.LastUpdated = DateTime.UtcNow;
            })
            //посылаем через Кафку в EventSourcingService -> CreateEventSourcingItemConsumer
            //Удаление записей по обновлению аукциона в сервисе BiddingService
            .Activity(p => p.OfType<BidActivity>())
            .TransitionTo(FinanceState));
    }

    // private void ConfigureFinanceState()
    // {
    //     During(FinanceState,
    //     When(AuctionDeletedFinanceEvent)
    //         .Then(context =>
    //         {
    //             context.Saga.LastUpdated = DateTime.UtcNow;
    //         })
    //         .Send(
    //             new Uri(configuration["QueuePaths:AuctionDeletingBid"]),
    //             context => new AuctionDeletingBid(
    //             context.Saga.AuctionId,
    //             context.Saga.CorrelationId))
    //         .TransitionTo(AuctionDeletedBidState));
    // }
    // private void ConfigureAuctionDeletedBid()
    // {
    //     During(AuctionDeletedBidState,
    //     When(AuctionDeletedBidEvent)
    //         .Then(context =>
    //         {
    //             context.Saga.LastUpdated = DateTime.UtcNow;
    //         })
    //         .Send(
    //             new Uri(configuration["QueuePaths:AuctionDeletingGateway"]),
    //             context => new AuctionDeletingGateway(
    //             context.Saga.AuctionId,
    //             context.Saga.CorrelationId))
    //         .TransitionTo(AuctionDeletedGatewayState));
    // }
    // private void ConfigureAuctionDeletedGateway()
    // {
    //     During(AuctionDeletedGatewayState,
    //     When(AuctionDeletedGatewayEvent)
    //         .Then(context =>
    //         {
    //             context.Saga.LastUpdated = DateTime.UtcNow;
    //         })
    //         .Send(
    //             new Uri(configuration["QueuePaths:AuctionDeletingImage"]),
    //             context => new AuctionDeletingImage(
    //             context.Saga.AuctionId,
    //             context.Saga.CorrelationId))
    //         .TransitionTo(AuctionDeletedImageState));
    // }
    // private void ConfigureAuctionDeletedImage()
    // {
    //     During(AuctionDeletedImageState,
    //     When(AuctionDeletedImageEvent)
    //         .Then(context =>
    //         {
    //             context.Saga.LastUpdated = DateTime.UtcNow;
    //         })
    //         .Send(
    //             new Uri(configuration["QueuePaths:AuctionDeletingSearch"]),
    //             context => new AuctionDeletingSearch(
    //             context.Saga.AuctionId,
    //             context.Saga.CorrelationId))
    //         .TransitionTo(AuctionDeletedSearchState));
    // }
    // private void ConfigureAuctionDeletedSearch()
    // {
    //     During(AuctionDeletedSearchState,
    //     When(AuctionDeletedSearchEvent)
    //         .Then(context =>
    //         {
    //             context.Saga.LastUpdated = DateTime.UtcNow;
    //         })
    //         .Send(
    //             new Uri(configuration["QueuePaths:AuctionDeletingNotification"]),
    //             context => new AuctionDeletingNotification(
    //             context.Saga.AuctionId,
    //             context.Saga.UserLogin,
    //             context.Saga.CorrelationId))
    //         .TransitionTo(AuctionDeletedNotificationState));
    // }
    // private void ConfigureAuctionDeletedNotification()
    // {
    //     During(AuctionDeletedNotificationState,
    //     When(AuctionDeletedNotificationEvent)
    //         .Then(context =>
    //         {
    //             context.Saga.LastUpdated = DateTime.UtcNow;
    //         })
    //         .Send(
    //             new Uri(configuration["QueuePaths:AuctionDeletingElk"]),
    //             context => new AuctionDeletingElk(
    //             context.Saga.AuctionId,
    //             context.Saga.UserLogin,
    //             context.Saga.CorrelationId))
    //         .TransitionTo(AuctionDeletedElkState));
    // }
    // private void ConfigureAuctionDeletedElk()
    // {
    //     During(AuctionDeletedElkState,
    //     When(AuctionDeletedElkEvent)
    //         .Then(context =>
    //         {
    //             context.Saga.LastUpdated = DateTime.UtcNow;
    //         })
    //         //.Activity(p => p.OfType<CommitDeletingAuctionActivity>())
    //         .TransitionTo(CommitAuctionDeletedState));
    // }

    // private void ConfigureCommitDeletingAuction()
    // {
    //     During(CommitAuctionDeletedState,
    //     When(CommitAuctionDeletedEvent)
    //         .Then(context =>
    //         {
    //             context.Saga.LastUpdated = DateTime.UtcNow;
    //         })
    //         .TransitionTo(CompletedState));
    // }

    // private void ConfigureCompleted()
    // {
    //     During(CompletedState);
    // }


    // private void ConfigureGetState()
    // {
    //     DuringAny(
    //         When(AuctionDeletedStateEvent)
    //             .Respond(x => x.Saga)
    //     );
    // }

}