using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcessingService.StateMachines.FinanceStateMachine;

namespace ProcessingService.Data;

public class FinanceStateMap : SagaClassMap<FinanceState>
{
    protected override void Configure(EntityTypeBuilder<FinanceState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64).IsRequired(true);
    }
}