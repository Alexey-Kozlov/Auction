using Common.Contracts.Processing;

namespace ProcessingService.StateMachines.CreateTagStateMachine;

public class CreateTagState : BaseProcessingState
{
    public string Name { get; set; }
    public Guid AuctionId { get; set; }
    public int CommitCounter { get; set; }
}
