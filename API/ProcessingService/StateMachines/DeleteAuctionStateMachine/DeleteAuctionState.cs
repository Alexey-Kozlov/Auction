using Common.Contracts.Processing;

namespace ProcessingService.StateMachines.DeleteAuctionStateMachine;

public class DeleteAuctionState : BaseProcessingState
{
    public string DataForProcessingServicesList { get; set; }
    public int CommitCounter { get; set; }
}
