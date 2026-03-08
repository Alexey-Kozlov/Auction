using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcessingService.StateMachines.DeleteTagStateMachine;

namespace ProcessingService.Data;

public class DeleteTagStateMap : SagaClassMap<DeleteTagState>
{
    protected override void Configure(EntityTypeBuilder<DeleteTagState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64).IsRequired(true);
    }
}