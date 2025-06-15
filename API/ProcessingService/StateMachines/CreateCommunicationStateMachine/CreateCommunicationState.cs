using MassTransit;

namespace ProcessingService.StateMachines.CreateCommunicationStateMachine;

public class CreateCommunicationState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public string UserLogin { get; set; }
    public Guid AuctionId { get; set; }
    public Guid ItemId { get; set; }
    public string Message { get; set; }
    public Guid? ParentId { get; set; }
    public bool IsError { get; set; }
    public string DataForProcessingServicesList { get; set; }
    public int CommitCounter { get; set; }
    public string SessionId { get; set; }
}
