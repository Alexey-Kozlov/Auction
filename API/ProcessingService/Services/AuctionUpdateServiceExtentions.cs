using Common.Contracts;
using MassTransit;
using ProcessingService.Data;
using ProcessingService.StateMachines.UpdateAuctionStateMachine;

namespace ProcessingService.Services;

public static class AuctionUpdateServiceExtentions
{
    public static void AddAuctionUpdateMassTransitConfigurator(this IBusRegistrationConfigurator conf)
    {
        conf.AddSagaStateMachine<UpdateAuctionStateMachine, UpdateAuctionState>((context, cfg) =>
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
    public static IServiceCollection AddAuctionUpdateServices(this IServiceCollection services, WebApplicationBuilder builder)
    {
        EndpointConvention.Map<AuctionUpdating>(new Uri("queue:auction-auction-updating"));
        EndpointConvention.Map<AuctionUpdatingBid>(new Uri("queue:bids-auction-updating-bid"));
        EndpointConvention.Map<AuctionUpdatingGateway>(new Uri("queue:gateway-auction-updating-gateway"));
        EndpointConvention.Map<AuctionUpdatingImage>(new Uri("queue:image-auction-updating-image"));
        EndpointConvention.Map<AuctionUpdatingSearch>(new Uri("queue:search-auction-updating-search"));
        EndpointConvention.Map<AuctionUpdatingNotification>(new Uri("queue:notification-auction-updating-notification"));
        EndpointConvention.Map<AuctionUpdatingElk>(new Uri("queue:elk-auction-updating-elk"));

        return services;
    }
}