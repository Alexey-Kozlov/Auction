using Common.Contracts.Processing;
using Common.Contracts.Tag;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.CreateTagStateMachine;

namespace ProcessingService.Activities.TagCreate;

public class ESLogActivity : IStateMachineActivity<CreateTagState, RequestCreateTag>
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

    public async Task Execute(BehaviorContext<CreateTagState, RequestCreateTag> context, IBehavior<CreateTagState, RequestCreateTag> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new TagItem
            {
                ItemId = context.Saga.ItemId.Value,
                AuctionId = context.Saga.AuctionId,
                Tag = context.Saga.Tag,
                CorrelationId = context.Saga.CorrelationId
            },
            nameof(TagItem),
            "Common.Contracts.Processing.ESLogTagCreated",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            Command.TagCreate,
            "",
            context.Saga.AuctionId,
            context.Saga.ItemId,
            false, "", "", "", "");
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<CreateTagState, RequestCreateTag, TException> context, IBehavior<CreateTagState, RequestCreateTag> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("scope");
    }
}