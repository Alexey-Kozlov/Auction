using MassTransit;
using ProcessingService.Data;
using ProcessingService.StateMachines.UpdateCommunicationStateMachine;

namespace ProcessingService.Services;

public static class CommunicationUpdateServiceExtentions
{
    public static void AddCommunicationUpdateMassTransitConfigurator(this IBusRegistrationConfigurator conf)
    {
        conf.AddSagaStateMachine<UpdateCommunicationStateMachine, UpdateCommunicationState>((context, cfg) =>
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