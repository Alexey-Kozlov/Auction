using MassTransit;
using ProcessingService.Data;
using ProcessingService.StateMachines.DeleteTagStateMachine;

namespace ProcessingService.Services;

public static class DeleteTagServiceExtentions
{
    public static void DeleteTagMassTransitConfigurator(this IBusRegistrationConfigurator conf)
    {
        conf.AddSagaStateMachine<DeleteTagStateMachine, DeleteTagState>((context, cfg) =>
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