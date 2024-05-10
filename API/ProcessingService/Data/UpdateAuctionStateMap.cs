using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcessingService.StateMachines.UpdateAuctionStateMachine;

namespace ProcessingService.Data;

public class UpdateAuctionStateMap : SagaClassMap<UpdateAuctionState>
{
    protected override void Configure(EntityTypeBuilder<UpdateAuctionState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64).IsRequired(true);
    }
}