using MassTransit;

namespace ProcessingService.StateMachines.RestoreStateMachine;

public class RestoreState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public Guid? ItemId { get; set; }
    public string CurrentState { get; set; }
    public DateTime RestoreDate { get; set; }
    public string UserLogin { get; set; }
    public bool ResetLog { get; set; }
    public string DataForProcessingServicesList { get; set; }
    public string NotifyMessage { get; set; }
    public float AllItemsCount { get; set; }
    public float ItemsCount { get; set; }
    public float ProgressCurrent { get; set; }
    public bool IsError { get; set; }
    public int CommitCounter { get; set; }
}
