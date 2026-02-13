using Common.Contracts.Processing;

namespace ProcessingService.StateMachines.BidPlacedStateMachine;

public class BidPlacedState : BaseProcessingState
{
    public string Bidder { get; set; }
    public Guid AuctionId { get; set; }
    public int Amount { get; set; }
    public int OldHighBid { get; set; }
    public string DataForProcessingServicesList { get; set; }
    public int CommitCounter { get; set; }
}
