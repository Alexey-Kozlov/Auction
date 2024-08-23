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
}