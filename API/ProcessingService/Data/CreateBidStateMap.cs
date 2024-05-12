using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcessingService.StateMachines.BidPlacedStateMachine;

namespace ProcessingService.Data;

public class CreateBidStateMap : SagaClassMap<BidPlacedState>
{
    protected override void Configure(EntityTypeBuilder<BidPlacedState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64).IsRequired(true);
        entity.Property(x => x.Bidder).IsRequired(true);
    }
}