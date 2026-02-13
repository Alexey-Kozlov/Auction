using Common.Contracts.Processing;

namespace ProcessingService.StateMachines.UpdateAuctionStateMachine;

public class UpdateAuctionState : BaseProcessingState
{
    public Guid AuctionId { get; set; }
    public string Title { get; set; }
    public string Properties { get; set; }
    public string Description { get; set; }
    public DateTime AuctionEnd { get; set; }
    public string Image { get; set; }
    public bool UsingImage { get; set; }
    public bool IsImageSplitted { get; set; }
    public string DataForProcessingServicesList { get; set; }
    public int CommitCounter { get; set; }
}
