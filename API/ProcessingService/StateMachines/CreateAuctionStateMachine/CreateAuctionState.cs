using Common.Contracts.Processing;

namespace ProcessingService.StateMachines.CreateAuctionStateMachine;

public class CreateAuctionState : BaseProcessingState
{
    public string Title { get; set; }
    public string Properties { get; set; }
    public string Description { get; set; }
    public DateTime AuctionEnd { get; set; }
    public int ReservePrice { get; set; }
    public string DataForProcessingServicesList { get; set; }
    public string Image { get; set; }
    public bool UsingImage { get; set; }
    public bool IsImageSplitted { get; set; }
    public int CommitCounter { get; set; }
}
