using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcessingService.StateMachines.CurrentSettingsStateMachine;

namespace ProcessingService.Data;

public class CurrentSettingsStateMap : SagaClassMap<CurrentSettingsState>
{
    protected override void Configure(EntityTypeBuilder<CurrentSettingsState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64).IsRequired(true);
    }
}