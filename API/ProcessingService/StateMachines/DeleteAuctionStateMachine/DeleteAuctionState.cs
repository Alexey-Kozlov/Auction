using MassTransit;

namespace ProcessingService.StateMachines.DeleteAuctionStateMachine;

public record DeleteAuctionState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public Guid AuctionId { get; set; }
    public string UserLogin { get; set; }
    public DateTime LastUpdated { get; set; }
    public string ErrorMessage { get; set; }
}
