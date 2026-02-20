using MassTransit;
using ProcessingService.Data;
using ProcessingService.StateMachines.CurrentSettingsStateMachine;

namespace ProcessingService.Services;

public static class CurrentStateServiceExtentions
{
    public static void CurrentStateMassTransitConfigurator(this IBusRegistrationConfigurator conf)
    {
        conf.AddSagaStateMachine<CurrentSettingsStateMachine, CurrentSettingsState>((context, cfg) =>
        {
            cfg.UseInMemoryOutbox(context);
        })
        .EntityFrameworkRepository(p =>
        {
            p.ConcurrencyMode = ConcurrencyMode.Optimistic;
            p.ExistingDbContext<ProcessingDbContext>();
            p.UsePostgres();
        });
    }
}