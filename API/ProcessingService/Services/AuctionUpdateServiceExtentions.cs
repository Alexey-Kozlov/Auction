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
}