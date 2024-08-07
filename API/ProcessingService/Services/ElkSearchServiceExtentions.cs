using Common.Contracts;
using Common.Utils;
using MassTransit;
using ProcessingService.Data;
using ProcessingService.StateMachines.ElkSearchStateMachine;

namespace ProcessingService.Services;

public static class ElkSearchServiceExtentions
{
    public static void ElkSearchMassTransitConfigurator(this IBusRegistrationConfigurator conf)
    {
        conf.AddSagaStateMachine<ElkSearchStateMachine, ElkSearchState>((context, cfg) =>
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
    public static void ElkSearchCreateServices(this IServiceCollection services)
    {
        EndpointConvention.Map<ElkSearchCreating>(new Uri("queue:elk-search-creating-elk"));
        EndpointConvention.Map<ElkSearchResponse<ApiResponse<PagedResult<List<AuctionCreatingElk>>>>>(new Uri("queue:notification-elk-search-creating-notification"));
        return;
    }
}