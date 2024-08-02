using Common.Contracts;
using MassTransit;
using ProcessingService.Data;
using ProcessingService.StateMachines.DeleteAuctionStateMachine;
using ProcessingService.StateMachines.UpdateAuctionStateMachine;

namespace ProcessingService.Services;

public static class AuctionDeleteServiceExtentions
{
    public static void AddAuctionDeleteMassTransitConfigurator(this IBusRegistrationConfigurator conf)
    {
        conf.AddSagaStateMachine<DeleteAuctionStateMachine, DeleteAuctionState>((context, cfg) =>
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
    public static IServiceCollection AddAuctionDeleteServices(this IServiceCollection services, WebApplicationBuilder builder)
    {
        EndpointConvention.Map<AuctionDeleting>(new Uri("queue:auction-auction-deleting"));
        EndpointConvention.Map<AuctionDeletingFinance>(new Uri("queue:finance-auction-deleting-finance"));
        EndpointConvention.Map<AuctionDeletingBid>(new Uri("queue:bids-auction-deleting-bid"));
        EndpointConvention.Map<AuctionDeletingGateway>(new Uri("queue:gateway-auction-deleting-gateway"));
        EndpointConvention.Map<AuctionDeletingImage>(new Uri("queue:image-auction-deleting-image"));
        EndpointConvention.Map<AuctionDeletingSearch>(new Uri("queue:search-auction-deleting-search"));
        EndpointConvention.Map<AuctionDeletingNotification>(new Uri("queue:notification-auction-deleting-notification"));
        EndpointConvention.Map<AuctionDeletingElk>(new Uri("queue:elk-auction-deleting-elk"));

        return services;
    }
}