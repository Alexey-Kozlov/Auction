using MassTransit;
using ProcessingService.Data;
using ProcessingService.StateMachines.RestoreStateMachine;

namespace ProcessingService.Services;

public static class RestoreServiceExtentions
{
    public static void RestoreMassTransitConfigurator(this IBusRegistrationConfigurator conf)
    {
        conf.AddSagaStateMachine<RestoreStateMachine, RestoreState>((context, cfg) =>
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