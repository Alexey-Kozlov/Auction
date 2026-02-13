using Common.Contracts.Processing;

namespace ProcessingService.StateMachines.CreateCommunicationStateMachine;

public class CreateCommunicationState : BaseProcessingState
{
    public Guid AuctionId { get; set; }
    public string Message { get; set; }
    public Guid? ParentId { get; set; }
    public string DataForProcessingServicesList { get; set; }
    public int CommitCounter { get; set; }
}
