using MassTransit;

namespace ProcessingService.StateMachines.FinanceStateMachine;

public class FinanceState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public string UserLogin { get; set; }
    public int Amount { get; set; }
    public string SessionId { get; set; }
    public bool IsError { get; set; }
    public int CommitCounter { get; set; }
}
