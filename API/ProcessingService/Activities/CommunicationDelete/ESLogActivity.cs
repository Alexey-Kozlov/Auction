using System.Text.Json;
using Common.Contracts.Communication;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.DeleteCommunicationStateMachine;

namespace ProcessingService.Activities.CommunicationDelete;

public class ESLogActivity : IStateMachineActivity<DeleteCommunicationState, RequestCommunicationDelete>
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

    public async Task Execute(BehaviorContext<DeleteCommunicationState, RequestCommunicationDelete> context, IBehavior<DeleteCommunicationState, RequestCommunicationDelete> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new CommunicationItem
            {
                CorrelationId = context.Saga.CorrelationId,
                ItemId = context.Saga.ItemId,
                AuctionId = context.Saga.AuctionId,
                UserLogin = context.Saga.UserLogin,
            },
            nameof(CommunicationItem),
            "Common.Contracts.Processing.ESLogCommunicationDeleted",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            Command.CommunicationDelete,
            JsonSerializer.Serialize(context.Message),
            context.Saga.AuctionId,
            context.Saga.ItemId,
            false, "", "", "", "");
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<DeleteCommunicationState, RequestCommunicationDelete, TException> context, IBehavior<DeleteCommunicationState, RequestCommunicationDelete> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("scope");
    }
}