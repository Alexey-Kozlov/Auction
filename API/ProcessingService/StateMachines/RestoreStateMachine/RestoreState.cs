using Common.Contracts.Processing;

namespace ProcessingService.StateMachines.RestoreStateMachine;

public class RestoreState : BaseProcessingState
{
    public DateTime RestoreDate { get; set; }
    public bool ResetLog { get; set; }
    public string DataForProcessingServicesList { get; set; }
    public string NotifyMessage { get; set; }
    public float AllItemsCount { get; set; }
    public float ItemsCount { get; set; }
    public float ProgressCurrent { get; set; }
    public int CommitCounter { get; set; }
}
