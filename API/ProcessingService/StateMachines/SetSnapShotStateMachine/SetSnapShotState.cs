using Common.Contracts.Processing;

namespace ProcessingService.StateMachines.SetSnapShotStateMachine;

public class SetSnapShotState : BaseProcessingState
{
    public string NotifyMessage { get; set; }
    public int BatchCounter { get; set; }
    public float AllItemsCount { get; set; }
    public float ProgressCurrent { get; set; }
    public DateTime ActionDate { get; set; }
    public int CommitCounter { get; set; }
}
