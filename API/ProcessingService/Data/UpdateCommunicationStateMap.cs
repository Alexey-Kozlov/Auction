using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcessingService.StateMachines.UpdateCommunicationStateMachine;

namespace ProcessingService.Data;

public class UpdateCommunicationStateMap : SagaClassMap<UpdateCommunicationState>
{
    protected override void Configure(EntityTypeBuilder<UpdateCommunicationState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64).IsRequired(true);
    }
}