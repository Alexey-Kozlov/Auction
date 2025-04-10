using MassTransit;

namespace ProcessingService.StateMachines.EditNotificationStateMachine;

public class EditNotificationState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public string UserLogin { get; set; }
    public Guid AuctionId { get; set; }
    public bool Enable { get; set; }
    public string SessionId { get; set; }
    public string DataForProcessingServicesList { get; set; }
    public bool IsError { get; set; }
    public int CommitCounter { get; set; }
}
