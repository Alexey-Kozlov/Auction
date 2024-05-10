using MassTransit;

namespace ProcessingService.StateMachines.CreateBidStateMachine;

public class CreateBidState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public string Bidder { get; set; }
    public Guid AuctionId { get; set; }
    public int Amount { get; set; }
    public DateTime LastUpdated { get; set; }
    public string ErrorMessage { get; set; }
}
