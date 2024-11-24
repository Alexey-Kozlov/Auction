using MassTransit;
using ProcessingService.Data;
using ProcessingService.StateMachines.EditNotificationStateMachine;

namespace ProcessingService.Services;

public static class EditNotificationServiceExtentions
{
    public static void EditNotificationMassTransitConfigurator(this IBusRegistrationConfigurator conf)
    {
        conf.AddSagaStateMachine<EditNotificationStateMachine, EditNotificationState>((context, cfg) =>
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