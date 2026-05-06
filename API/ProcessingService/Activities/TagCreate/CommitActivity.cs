using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using Common.Contracts.Tag;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.CreateTagStateMachine;

namespace ProcessingService.Activities.TagCreate;

public class CommitActivity : IStateMachineActivity<CreateTagState, TagCreateESCommit>
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


    public async Task Execute(BehaviorContext<CreateTagState, TagCreateESCommit> context, IBehavior<CreateTagState, TagCreateESCommit> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new RequestCommitESOperation(context.Saga.CorrelationId),
            nameof(CommitESOperation),
            "Common.Contracts.Tag.TagListCommit",
            context.Saga.CorrelationId,
            context.Saga.UserLogin,
            Command.TagCreate,
            "",
            context.Saga.AuctionId,
            context.Saga.ItemId,
            context.Saga.IsError,
            context.Message.ErrorMessage,
            context.Message.ErrorExceptionStack,
            context.Message.ErrorExceptionInputData,
            context.Message.ErrorServiceName);

        await _publishEndpoint.Publish(new TagCommit
        {
            Commited = !context.Saga.IsError,
            CallBackType = "Common.Contracts.Tag.TagListCommit",
            CorrelationId = context.Message.CorrelationId,
            ErrorMessage = context.Message.ErrorMessage,
            ErrorExceptionStack = context.Message.ErrorExceptionStack,
            ErrorExceptionInputData = context.Message.ErrorExceptionInputData,
            ErrorServiceName = context.Message.ErrorServiceName,
            UserLogin = context.Saga.UserLogin
        });

        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<CreateTagState, TagCreateESCommit, TException> context, IBehavior<CreateTagState, TagCreateESCommit> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("scope");
    }
}