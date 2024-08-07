using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcessingService.StateMachines.ElkSearchStateMachine;

namespace ProcessingService.Data;

public class ElkSearchStateMap : SagaClassMap<ElkSearchState>
{
    protected override void Configure(EntityTypeBuilder<ElkSearchState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64).IsRequired(true);
    }
}