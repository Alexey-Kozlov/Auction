using Common.Contracts.Processing;

namespace ProcessingService.StateMachines.EditNotificationStateMachine;

public class EditNotificationState : BaseProcessingState
{
    public bool Enable { get; set; }
    public string DataForProcessingServicesList { get; set; }
    public int CommitCounter { get; set; }
}
