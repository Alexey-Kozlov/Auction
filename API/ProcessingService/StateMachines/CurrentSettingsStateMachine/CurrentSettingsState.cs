using Common.Contracts.Processing;

namespace ProcessingService.StateMachines.CurrentSettingsStateMachine;

public class CurrentSettingsState : BaseProcessingState
{
    public bool AdminMode { get; set; }
}
