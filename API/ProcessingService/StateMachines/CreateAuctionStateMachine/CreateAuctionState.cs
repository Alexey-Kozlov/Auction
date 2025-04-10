using MassTransit;

namespace ProcessingService.StateMachines.CreateAuctionStateMachine;

public class CreateAuctionState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public Guid AuctionId { get; set; }
    public string Title { get; set; }
    public string Properties { get; set; }
    public string Description { get; set; }
    public string UserLogin { get; set; }
    public DateTime AuctionEnd { get; set; }
    public int ReservePrice { get; set; }
    public string DataForProcessingServicesList { get; set; }
    public string Image { get; set; }
    public bool UsingImage { get; set; }
    public bool IsImageSplitted { get; set; }
    public bool IsError { get; set; }
    public int CommitCounter { get; set; }
}
