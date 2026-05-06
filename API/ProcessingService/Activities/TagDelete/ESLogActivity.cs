using Common.Contracts.Processing;
using Common.Contracts.Tag;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.DeleteTagStateMachine;

namespace ProcessingService.Activities.TagDelete;

public class ESLogActivity : IStateMachineActivity<DeleteTagState, RequestDeleteTag>
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

    public async Task Execute(BehaviorContext<DeleteTagState, RequestDeleteTag> context, IBehavior<DeleteTagState, RequestDeleteTag> next)
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
            "Common.Contracts.Processing.ESLogTagDeleted",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            Command.TagDelete,
            "",
            context.Saga.AuctionId,
            context.Saga.ItemId,
            false, "", "", "", "");
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<DeleteTagState, RequestDeleteTag, TException> context, IBehavior<DeleteTagState, RequestDeleteTag> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("scope");
    }
}