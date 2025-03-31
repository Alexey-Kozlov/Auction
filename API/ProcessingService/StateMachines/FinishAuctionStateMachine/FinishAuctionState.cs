using MassTransit;

namespace ProcessingService.StateMachines.FinishAuctionStateMachine;

public record FinishAuctionState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public int Amount { get; set; }
    public string DataForProcessingServicesList { get; set; }
    public bool IsError { get; set; }
}
