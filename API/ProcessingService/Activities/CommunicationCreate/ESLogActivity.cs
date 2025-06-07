using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Communication;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.CreateCommunicationStateMachine;

namespace ProcessingService.Activities.CommunicationCreate;

public class ESLogActivity : IStateMachineActivity<CreateCommunicationState, RequestCommunicationCreate>
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

    public async Task Execute(BehaviorContext<CreateCommunicationState, RequestCommunicationCreate> context, IBehavior<CreateCommunicationState, RequestCommunicationCreate> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new CommunicationItem
            {
                AuctionId = context.Saga.AuctionId,
                Message = context.Saga.Message,
                UserLogin = context.Saga.UserLogin,
                ParentId = context.Saga.ParentId,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            },
            nameof(CommunicationItem),
            "Common.Contracts.Processing.ESLogCommunicationCreated",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            Command.CommunicationCreate,
            JsonSerializer.Serialize(context.Message),
            context.Saga.AuctionId,
            false, "", "", "");
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<CreateCommunicationState, RequestCommunicationCreate, TException> context, IBehavior<CreateCommunicationState, RequestCommunicationCreate> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-create");
    }
}