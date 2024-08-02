using Common.Contracts;
using MassTransit;
using ProcessingService.Data;
using ProcessingService.StateMachines.CreateAuctionStateMachine;

namespace ProcessingService.Services;

public static class AuctionCreateServiceExtentions
{
    public static void AddAuctionCreateMassTransitConfigurator(this IBusRegistrationConfigurator conf)
    {
        conf.AddSagaStateMachine<CreateAuctionStateMachine, CreateAuctionState>((context, cfg) =>
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
    public static IServiceCollection AddAuctionCreateServices(this IServiceCollection services, WebApplicationBuilder builder)
    {
        EndpointConvention.Map<AuctionCreating>(new Uri("queue:auction-auction-creating"));
        EndpointConvention.Map<AuctionCreatingBid>(new Uri("queue:bids-auction-creating-bid"));
        EndpointConvention.Map<AuctionCreatingImage>(new Uri("queue:image-auction-creating-image"));
        EndpointConvention.Map<AuctionCreatingSearch>(new Uri("queue:search-auction-creating-search"));
        EndpointConvention.Map<AuctionCreatingNotification>(new Uri("queue:notification-auction-creating-notification"));
        EndpointConvention.Map<AuctionCreatingElk>(new Uri("queue:elk-auction-creating-elk"));
        return services;
    }
}