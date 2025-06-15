using MassTransit;

namespace ProcessingService.StateMachines.DeleteAuctionStateMachine;

public record DeleteAuctionState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public Guid AuctionId { get; set; }
    public Guid? ItemId { get; set; }
    public string UserLogin { get; set; }
    public string DataForProcessingServicesList { get; set; }
    public bool IsError { get; set; }
    public int CommitCounter { get; set; }
}
