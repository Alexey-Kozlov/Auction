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
}