using System.Text.Json;
using Common.Contracts.Communication;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.UpdateCommunicationStateMachine;

namespace ProcessingService.Activities.CommunicationUpdate;

public class ESLogActivity : IStateMachineActivity<UpdateCommunicationState, RequestCommunicationUpdate>
{
    private readonly SendEventToES _sendEventToES;
    public ESLogActivity(SendEventToES sendEventToES)
    {
        _sendEventToES = sendEventToES;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<UpdateCommunicationState, RequestCommunicationUpdate> context, IBehavior<UpdateCommunicationState, RequestCommunicationUpdate> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new CommunicationItem
            {
                CorrelationId = context.Saga.CorrelationId,
                ItemId = context.Saga.ItemId,
                AuctionId = context.Saga.AuctionId,
                Message = context.Saga.Message,
                ParentId = context.Saga.ParentId,
                UserLogin = context.Saga.UserLogin,
            },
            nameof(CommunicationItem),
            "Common.Contracts.Processing.ESLogCommunicationUpdated",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            Command.CommunicationUpdate,
            JsonSerializer.Serialize(context.Message),
            context.Saga.AuctionId,
            context.Saga.ItemId,
            false, "", "", "", "");
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<UpdateCommunicationState, RequestCommunicationUpdate, TException> context, IBehavior<UpdateCommunicationState, RequestCommunicationUpdate> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("scope");
    }
}