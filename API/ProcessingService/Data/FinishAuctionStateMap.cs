using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcessingService.StateMachines.FinishAuctionStateMachine;

namespace ProcessingService.Data;

public class FinishAuctionStateMap : SagaClassMap<FinishAuctionState>
{
    protected override void Configure(EntityTypeBuilder<FinishAuctionState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64).IsRequired(true);
    }
}