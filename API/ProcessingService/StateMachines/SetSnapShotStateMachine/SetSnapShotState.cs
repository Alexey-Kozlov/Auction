using MassTransit;

namespace ProcessingService.StateMachines.SetSnapShotStateMachine;

public class SetSnapShotState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public DateTime LastUpdated { get; set; }
    public string UserLogin { get; set; }
    public string ErrorMessage { get; set; }
}
