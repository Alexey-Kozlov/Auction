using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Processing;
using MassTransit;
using ProcessingService.Activities.AuctionFinish;

namespace ProcessingService.StateMachines.FinishAuctionStateMachine;
public class FinishAuctionStateMachine : MassTransitStateMachine<FinishAuctionState>
{

    public State ElkState { get; }
    public State NotificationState { get; }
    public State CommitState { get; }
    public State CompletedState { get; }

    public Event<ESLog_AuctionFinish> EsLogEvent { get; }
    public Event<AuctionFinishedElk> ElkEvent { get; }
    public Event<AuctionFinishedNotification> NotificationEvent { get; }
    public Event<AuctionFinishedESCommit> CommitEvent { get; }
    public Event<AuctionFinishedComplete> CompleteEvent { get; }

    private IConfiguration configuration { get; }

    public FinishAuctionStateMachine(IServiceProvider services)
    {
        configuration = services.CreateScope().ServiceProvider.GetRequiredService<IConfiguration>();
        InstanceState(state => state.CurrentState);
        ConfigureEvents();
        ConfigureInitialState();
        ConfigureNotificationState();
        ConfigureELKState();
        ConfigureCommitState();
        ConfigureCompletedState();

    }
    private void ConfigureEvents()
    {
        Event(() => EsLogEvent, p => p.InsertOnInitial = true);
        Event(() => NotificationEvent);
        Event(() => ElkEvent);
        Event(() => CommitEvent);
        Event(() => CompleteEvent);
    }
    private void ConfigureInitialState()
    {
        //В сервисе EventSourcingService периодически срабатывает сервис CheckAuctionFinished, если найдены завершенные
        //аукционы - они завершаются и передается список аукционов для обнолвения в AuctionService
        Initially(
            When(EsLogEvent)
            .Then(context =>
            {
                context.Saga.CorrelationId = context.Message.CorrelationId;
                context.Saga.Amount = context.Message.DataItems.DataObjects.Count();
                context.Saga.DataForProcessingServicesList = JsonSerializer.Serialize(context.Message.DataItems);
            })
            //Обновление аукциона в сервисе SearchService
            .Send(
                new Uri(configuration["QueuePaths:SearchConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = context.Message.DataItems.DataObjects,
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionFinishedElk"
                })
            .TransitionTo(ElkState)
        );
        SetCompletedWhenFinalized();
    }

    private void ConfigureELKState()
    {
        During(ElkState,
        When(ElkEvent)
            //Обновление аукциона в поиске в сервисе ElasticSearchService
            .Send(
                new Uri(configuration["QueuePaths:ElkConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects,
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionFinishedNotification"
                })
            .TransitionTo(NotificationState));
    }

    private void ConfigureNotificationState()
    {
        During(NotificationState,
        When(NotificationEvent)
            .Send(
                new Uri(configuration["QueuePaths:AuctionFinishedNotificationConsumer"]),
                context => new DataForProcessingServicesList<AuctionItem>
                {
                    DataObjects = JsonSerializer.Deserialize<DataForProcessingServicesList>(context.Saga.DataForProcessingServicesList).DataObjects,
                    CorrelationId = context.Saga.CorrelationId,
                    CallBackType = "Common.Contracts.Auction.AuctionFinishedESCommit"
                })
            .TransitionTo(CommitState));
    }


    private void ConfigureCommitState()
    {
        During(CommitState,
        When(CommitEvent)
            //посылаем через Кафку в EventSourcingService - для подтверждения транзакции
            .Activity(p => p.OfType<CommitActivity>())
            .TransitionTo(CompletedState));
    }

    private void ConfigureCompletedState()
    {
        During(CompletedState,
        When(CompleteEvent).Finalize()
        );
    }



}