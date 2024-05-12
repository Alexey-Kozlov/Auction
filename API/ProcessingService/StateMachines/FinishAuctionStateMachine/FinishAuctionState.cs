using MassTransit;

namespace ProcessingService.StateMachines.FinishAuctionStateMachine;

public record FinishAuctionState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public Guid Id { get; set; }
    public string Winner { get; set; }
    public bool ItemSold { get; set; }
    public int Amount { get; set; }
    public DateTime LastUpdated { get; set; }
    public string ErrorMessage { get; set; }
}
