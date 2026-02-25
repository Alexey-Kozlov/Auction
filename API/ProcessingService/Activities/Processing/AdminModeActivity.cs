using Common.Contracts.Settings;
using Common.Utils.Settings;
using MassTransit;
using ProcessingService.StateMachines.CurrentSettingsStateMachine;

namespace ProcessingService.Activities.Processing;

public class AdminModeActivity : IStateMachineActivity<CurrentSettingsState, SetCurrentSettingsCompleted>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IsAdminModeService _isAdminModeService;
    public AdminModeActivity(IPublishEndpoint publishEndpoint, IsAdminModeService isAdminModeService)
    {
        _publishEndpoint = publishEndpoint;
        _isAdminModeService = isAdminModeService;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<CurrentSettingsState, SetCurrentSettingsCompleted> context, IBehavior<CurrentSettingsState, SetCurrentSettingsCompleted> next)
    {
        //обновляем состояние AdminMode в остальных сервисах
        await _publishEndpoint.Publish(new SetAdminMode
        {
            CorrelationId = context.Saga.CorrelationId,
            AdminMode = context.Saga.AdminMode
        });
        //обновляем состояние AdminMode в текущем сервисе Processing
        await Task.Run(() => _isAdminModeService.SetAdminMode(context.Saga.AdminMode));
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<CurrentSettingsState, SetCurrentSettingsCompleted, TException> context, IBehavior<CurrentSettingsState, SetCurrentSettingsCompleted> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction");
    }
}