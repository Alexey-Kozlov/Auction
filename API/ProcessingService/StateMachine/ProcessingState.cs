using MassTransit;

namespace ProcessingService.StateMachines;

public class ProcessingState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public string Bidder { get; set; }
    public Guid AuctionId { get; set; }
    public int Amount { get; set; }
    public DateTime LastUpdated { get; set; }
    public string ErrorMessage { get; set; }
}
