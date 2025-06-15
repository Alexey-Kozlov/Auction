using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcessingService.StateMachines.DeleteCommunicationStateMachine;

namespace ProcessingService.Data;

public class DeleteCommunicationStateMap : SagaClassMap<DeleteCommunicationState>
{
    protected override void Configure(EntityTypeBuilder<DeleteCommunicationState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64).IsRequired(true);
    }
}