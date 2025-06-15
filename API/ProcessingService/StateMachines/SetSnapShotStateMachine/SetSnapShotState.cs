using MassTransit;

namespace ProcessingService.StateMachines.SetSnapShotStateMachine;

public class SetSnapShotState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public Guid? ItemId { get; set; }
    public string CurrentState { get; set; }
    public string UserLogin { get; set; }
    public string NotifyMessage { get; set; }
    public int BatchCounter { get; set; }
    public float AllItemsCount { get; set; }
    public float ProgressCurrent { get; set; }
    public string SessionId { get; set; }
    public DateTime ActionDate { get; set; }
    public bool IsError { get; set; }
    public int CommitCounter { get; set; }
}
