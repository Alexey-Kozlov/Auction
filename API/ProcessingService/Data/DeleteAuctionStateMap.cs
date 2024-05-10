using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcessingService.StateMachines.DeleteAuctionStateMachine;

namespace ProcessingService.Data;

public class DeleteAuctionStateMap : SagaClassMap<DeleteAuctionState>
{
    protected override void Configure(EntityTypeBuilder<DeleteAuctionState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64).IsRequired(true);
    }
}