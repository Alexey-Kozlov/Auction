using Common.Contracts.Processing;

namespace ProcessingService.StateMachines.ElkIndexStateMachine;

public class ElkIndexState : BaseProcessingState
{
    public int ItemNumber { get; set; }
}
