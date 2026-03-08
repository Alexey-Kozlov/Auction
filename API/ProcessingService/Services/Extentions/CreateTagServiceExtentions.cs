using MassTransit;
using ProcessingService.Data;
using ProcessingService.StateMachines.CreateTagStateMachine;

namespace ProcessingService.Services;

public static class CreateTagServiceExtentions
{
    public static void CreateTagMassTransitConfigurator(this IBusRegistrationConfigurator conf)
    {
        conf.AddSagaStateMachine<CreateTagStateMachine, CreateTagState>((context, cfg) =>
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