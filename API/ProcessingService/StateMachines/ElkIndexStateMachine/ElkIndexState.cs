using MassTransit;

namespace ProcessingService.StateMachines.ElkIndexStateMachine;

public class ElkIndexState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public string UserLogin { get; set; }
    public string SessionId { get; set; }
    public int ItemNumber { get; set; }
}
