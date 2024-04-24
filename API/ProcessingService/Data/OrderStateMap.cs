using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcessingService.StateMachines;

public class OrderStateMap : SagaClassMap<ProcessingState>
{
    protected override void Configure(EntityTypeBuilder<ProcessingState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64).IsRequired(true);
        entity.Property(x => x.Bidder).IsRequired(true);
    }
}