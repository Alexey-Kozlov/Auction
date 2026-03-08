using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcessingService.StateMachines.CreateTagStateMachine;

namespace ProcessingService.Data;

public class CreateTagStateMap : SagaClassMap<CreateTagState>
{
    protected override void Configure(EntityTypeBuilder<CreateTagState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64).IsRequired(true);
    }
}