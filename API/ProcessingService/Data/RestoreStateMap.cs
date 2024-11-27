using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcessingService.StateMachines.RestoreStateMachine;

namespace ProcessingService.Data;

public class RestoreStateMap : SagaClassMap<RestoreState>
{
    protected override void Configure(EntityTypeBuilder<RestoreState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64).IsRequired(true);
    }
}