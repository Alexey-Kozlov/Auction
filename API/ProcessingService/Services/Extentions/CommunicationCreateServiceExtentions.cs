using MassTransit;
using ProcessingService.Data;
using ProcessingService.StateMachines.CreateCommunicationStateMachine;

namespace ProcessingService.Services;

public static class CommunicationCreateServiceExtentions
{
    public static void AddCommunicationCreateMassTransitConfigurator(this IBusRegistrationConfigurator conf)
    {
        conf.AddSagaStateMachine<CreateCommunicationStateMachine, CreateCommunicationState>((context, cfg) =>
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