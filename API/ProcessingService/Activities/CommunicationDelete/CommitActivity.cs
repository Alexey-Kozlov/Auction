using Common.Contracts.Communication;
using Common.Contracts.ELKSearch;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.DeleteCommunicationStateMachine;

namespace ProcessingService.Activities.CommunicationDelete;

public class CommitActivity : IStateMachineActivity<DeleteCommunicationState, CommunicationDeleteESCommit>
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


    public async Task Execute(BehaviorContext<DeleteCommunicationState, CommunicationDeleteESCommit> context, IBehavior<DeleteCommunicationState, CommunicationDeleteESCommit> next)
    {
        //фиксируем транзакцию в EventsLog
        await _sendEventToES.SendItemToEventSourcing(
            new RequestCommitESOperation(context.Saga.CorrelationId),
            nameof(CommitESOperation),
            "Common.Contracts.Communication.CommunicationDeleteNotificationEvent",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            Command.CommunicationDelete,
            "",
            context.Saga.AuctionId,
            context.Saga.ItemId,
            context.Saga.IsError,
            context.Message.ErrorMessage,
            context.Message.ErrorExceptionMessage,
            context.Message.ErrorServiceName);

        //фиксируем транзакцию в БД чтения CommunicationService
        await _publishEndpoint.Publish(new CommunicationCommit
        {
            Commited = !context.Saga.IsError,
            CallBackType = "Common.Contracts.Communication.CommunicationDeleteNotificationEvent",
            CorrelationId = context.Saga.CorrelationId,
            ErrorMessage = context.Message.ErrorMessage,
            ErrorExceptionMessage = context.Message.ErrorExceptionMessage,
            ErrorServiceName = context.Message.ErrorServiceName,
            UserLogin = context.Saga.UserLogin
        });

        //фиксируем транзакцию в сервисе поиска
        await _publishEndpoint.Publish(new ElkCommit
        {
            Commited = !context.Saga.IsError,
            CallBackType = "Common.Contracts.Communication.CommunicationDeleteNotificationEvent",
            CorrelationId = context.Saga.CorrelationId,
            ErrorMessage = context.Message.ErrorMessage,
            ErrorExceptionMessage = context.Message.ErrorExceptionMessage,
            ErrorServiceName = context.Message.ErrorServiceName,
            UserLogin = context.Saga.UserLogin,
            ElkIndex = "communication_index"
        });

        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<DeleteCommunicationState, CommunicationDeleteESCommit, TException> context, IBehavior<DeleteCommunicationState, CommunicationDeleteESCommit> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-communication-delete");
    }
}