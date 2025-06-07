using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcessingService.StateMachines.CreateCommunicationStateMachine;

namespace ProcessingService.Data;

public class CreateCommunicationStateMap : SagaClassMap<CreateCommunicationState>
{
    protected override void Configure(EntityTypeBuilder<CreateCommunicationState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64).IsRequired(true);
    }
}