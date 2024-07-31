using Contracts;
using MassTransit;
using ProcessingService.Data;
using ProcessingService.StateMachines.FinishAuctionStateMachine;

namespace ProcessingService.Services;

public static class AuctionFinishServiceExtentions
{
    public static void AddAuctionFinishMassTransitConfigurator(this IBusRegistrationConfigurator conf)
    {
        conf.AddSagaStateMachine<FinishAuctionStateMachine, FinishAuctionState>((context, cfg) =>
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
    public static IServiceCollection AddAuctionFinishServices(this IServiceCollection services, WebApplicationBuilder builder)
    {
        EndpointConvention.Map<AuctionFinishing>(new Uri("queue:auction-auction-finishing"));
        EndpointConvention.Map<AuctionFinishingFinance>(new Uri("queue:finance-auction-finishing-finance"));
        EndpointConvention.Map<AuctionFinishingSearch>(new Uri("queue:search-auction-finishing-search"));
        EndpointConvention.Map<AuctionFinishingNotification>(new Uri("queue:notification-auction-finishing-notification"));
        EndpointConvention.Map<AuctionFinishingElk>(new Uri("queue:elk-auction-finishing-elk"));


        return services;
    }
}