using Common.Contracts.Processing;

namespace ProcessingService.StateMachines.FinanceStateMachine;

public class FinanceState : BaseProcessingState
{
    public int Amount { get; set; }
    public int CommitCounter { get; set; }
}
