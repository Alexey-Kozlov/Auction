using Contracts;
using MassTransit;
using ProcessingService.Data;
using ProcessingService.StateMachines.BidPlacedStateMachine;

namespace ProcessingService.Services;

public static class BidCreateServiceExtentions
{
    public static void AddBidPlacedMassTransitConfigurator(this IBusRegistrationConfigurator conf)
    {
        conf.AddSagaStateMachine<BidPlacedStateMachine, BidPlacedState>((context, cfg) =>
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
    public static IServiceCollection AddBidPlacedServices(this IServiceCollection services, WebApplicationBuilder builder)
    {
        EndpointConvention.Map<BidFinanceGranting>(new Uri("queue:finance-bid-finance-granting"));
        EndpointConvention.Map<BidAuctionPlacing>(new Uri("queue:auction-bid-auction-placing"));
        EndpointConvention.Map<BidPlacing>(new Uri("queue:bids-bid-placing"));
        EndpointConvention.Map<BidSearchPlacing>(new Uri("queue:search-bid-search-placing"));
        EndpointConvention.Map<BidNotificationProcessing>(new Uri("queue:notification-bid-notification-processing"));
        EndpointConvention.Map<RollbackBidFinanceGranted>(new Uri("queue:finance-rollback-bid-finance-granted"));
        EndpointConvention.Map<RollbackBidAuctionPlaced>(new Uri("queue:auction-rollback-bid-auction-placed"));
        EndpointConvention.Map<RollbackBidPlaced>(new Uri("queue:bids-rollback-bid-placed"));
        return services;
    }
}