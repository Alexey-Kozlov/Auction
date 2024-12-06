using MassTransit;
using ProcessingService.Data;
using ProcessingService.StateMachines.SetSnapShotStateMachine;

namespace ProcessingService.Services;

public static class SetSnapShotServiceExtentions
{
    public static void SetSnapShotMassTransitConfigurator(this IBusRegistrationConfigurator conf)
    {
        conf.AddSagaStateMachine<SetSnapShotStateMachine, SetSnapShotState>((context, cfg) =>
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