using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcessingService.StateMachines.CreateAuctionStateMachine;

namespace ProcessingService.Data;

public class CreateAuctionStateMap : SagaClassMap<CreateAuctionState>
{
    protected override void Configure(EntityTypeBuilder<CreateAuctionState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64).IsRequired(true);
    }
}