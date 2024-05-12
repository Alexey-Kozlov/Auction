using MassTransit;

namespace ProcessingService.StateMachines.CreateAuctionStateMachine;

public class CreateAuctionState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Properties { get; set; }
    public string Description { get; set; }
    public string AuctionAuthor { get; set; }
    public DateTime AuctionEnd { get; set; }
    public DateTime LastUpdated { get; set; }
    public string ErrorMessage { get; set; }
    public string Image { get; set; }
    public int ReservePrice { get; set; }
}
