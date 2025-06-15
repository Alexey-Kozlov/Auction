using MassTransit;
using ProcessingService.Data;
using ProcessingService.StateMachines.DeleteCommunicationStateMachine;

namespace ProcessingService.Services;

public static class CommunicationDeleteServiceExtentions
{
    public static void AddCommunicationDeleteMassTransitConfigurator(this IBusRegistrationConfigurator conf)
    {
        conf.AddSagaStateMachine<DeleteCommunicationStateMachine, DeleteCommunicationState>((context, cfg) =>
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