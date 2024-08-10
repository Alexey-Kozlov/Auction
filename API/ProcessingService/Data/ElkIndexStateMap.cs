using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcessingService.StateMachines.ElkIndexStateMachine;

namespace ProcessingService.Data;

public class ElkIndexStateMap : SagaClassMap<ElkIndexState>
{
    protected override void Configure(EntityTypeBuilder<ElkIndexState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64).IsRequired(true);
    }
}