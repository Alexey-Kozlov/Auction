using Common.Contracts.Processing;

namespace ProcessingService.StateMachines.DeleteTagStateMachine;

public class DeleteTagState : BaseProcessingState
{
    public string Tag { get; set; }
    public Guid AuctionId { get; set; }
    public int CommitCounter { get; set; }
}
