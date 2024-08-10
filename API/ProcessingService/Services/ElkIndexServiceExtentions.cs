using Common.Contracts;
using Common.Utils;
using MassTransit;
using ProcessingService.Data;
using ProcessingService.StateMachines.ElkIndexStateMachine;

namespace ProcessingService.Services;

public static class ElkIndexServiceExtentions
{
    public static void ElkIndexMassTransitConfigurator(this IBusRegistrationConfigurator conf)
    {
        conf.AddSagaStateMachine<ElkIndexStateMachine, ElkIndexState>((context, cfg) =>
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
    public static void ElkIndexCreateServices(this IServiceCollection services)
    {
        EndpointConvention.Map<ElkIndexCreating>(new Uri("queue:elk-elk-index"));
        EndpointConvention.Map<ElkIndexResponse>(new Uri("queue:notification-elk-index-creating-notification"));
        return;
    }
}