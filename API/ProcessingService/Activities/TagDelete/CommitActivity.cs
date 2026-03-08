using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using Common.Contracts.Tag;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.DeleteTagStateMachine;

namespace ProcessingService.Activities.TagDelete;

public class CommitActivity : IStateMachineActivity<DeleteTagState, TagDeleteESCommit>
{
    private readonly SendEventToES _sendEventToES;
    private readonly IPublishEndpoint _publishEndpoint;

    public CommitActivity(SendEventToES sendEventToES, IPublishEndpoint publishEndpoint)
    {
        _sendEventToES = sendEventToES;
        _publishEndpoint = publishEndpoint;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }


    public async Task Execute(BehaviorContext<DeleteTagState, TagDeleteESCommit> context, IBehavior<DeleteTagState, TagDeleteESCommit> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new RequestCommitESOperation(context.Saga.CorrelationId),
            nameof(CommitESOperation),
            "Common.Contracts.Tag.TagListCommit",
            context.Saga.CorrelationId,
            context.Saga.UserLogin,
            Command.TagDelete,
            "",
            context.Saga.AuctionId,
            context.Saga.ItemId,
            context.Saga.IsError,
            context.Message.ErrorMessage,
            context.Message.ErrorExceptionMessage,
            context.Message.ErrorServiceName);

        await _publishEndpoint.Publish(new TagCommit
        {
            Commited = !context.Saga.IsError,
            CallBackType = "Common.Contracts.Tag.TagListCommit",
            CorrelationId = context.Message.CorrelationId,
            ErrorMessage = context.Message.ErrorMessage,
            ErrorExceptionMessage = context.Message.ErrorExceptionMessage,
            ErrorServiceName = context.Message.ErrorServiceName,
            UserLogin = context.Saga.UserLogin
        });

        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<DeleteTagState, TagDeleteESCommit, TException> context, IBehavior<DeleteTagState, TagDeleteESCommit> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-update");
    }
}