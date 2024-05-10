using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcessingService.StateMachines.CreateBidStateMachine;

namespace ProcessingService.Data;

public class CreateBidStateMap : SagaClassMap<CreateBidState>
{
    protected override void Configure(EntityTypeBuilder<CreateBidState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64).IsRequired(true);
        entity.Property(x => x.Bidder).IsRequired(true);
    }
}