using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcessingService.StateMachines.SetSnapShotStateMachine;

namespace ProcessingService.Data;

public class SetSnapShotStateMap : SagaClassMap<SetSnapShotState>
{
    protected override void Configure(EntityTypeBuilder<SetSnapShotState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64).IsRequired(true);
    }
}