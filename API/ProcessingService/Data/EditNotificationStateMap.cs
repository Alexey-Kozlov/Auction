using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcessingService.StateMachines.EditNotificationStateMachine;

namespace ProcessingService.Data;

public class EditNotificationStateMap : SagaClassMap<EditNotificationState>
{
    protected override void Configure(EntityTypeBuilder<EditNotificationState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64).IsRequired(true);
    }
}