using MassTransit;

namespace ProcessingService.StateMachines.RestoreStateMachine;

public class RestoreState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public DateTime RestoreDate { get; set; }
    public DateTime LastUpdated { get; set; }
    public string UserLogin { get; set; }
    public string ErrorMessage { get; set; }
    public bool ResetLog { get; set; }
}
