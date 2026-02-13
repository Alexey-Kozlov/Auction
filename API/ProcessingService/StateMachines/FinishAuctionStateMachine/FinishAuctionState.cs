using Common.Contracts.Processing;

namespace ProcessingService.StateMachines.FinishAuctionStateMachine;

public class FinishAuctionState : BaseProcessingState
{
    public int Amount { get; set; }
    public string DataForProcessingServicesList { get; set; }
    public int CommitCounter { get; set; }
}
