using Common.Contracts.Processing;

namespace ProcessingService.StateMachines.UpdateCommunicationStateMachine;

public class UpdateCommunicationState : BaseProcessingState
{
    public string Message { get; set; }
    public Guid? ParentId { get; set; }
    public Guid? AuctionId { get; set; }
    public string DataForProcessingServicesList { get; set; }
    public int CommitCounter { get; set; }
}
