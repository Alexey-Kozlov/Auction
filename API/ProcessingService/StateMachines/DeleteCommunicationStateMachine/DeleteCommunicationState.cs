using Common.Contracts.Processing;

namespace ProcessingService.StateMachines.DeleteCommunicationStateMachine;

public class DeleteCommunicationState : BaseProcessingState
{
    public Guid? AuctionId { get; set; }
    public string DataForProcessingServicesList { get; set; }
    public int CommitCounter { get; set; }
}
