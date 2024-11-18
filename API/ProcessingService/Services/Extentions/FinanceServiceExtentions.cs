using MassTransit;
using ProcessingService.Data;
using ProcessingService.StateMachines.FinanceStateMachine;


namespace ProcessingService.Services;

public static class FinanceServiceExtentions
{
    public static void FinanceMassTransitConfigurator(this IBusRegistrationConfigurator conf)
    {
        conf.AddSagaStateMachine<FinanceStateMachine, FinanceState>((context, cfg) =>
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