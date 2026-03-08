using Common.Contracts.Auction;
using Common.Contracts.Bid;
using Common.Contracts.Communication;
using Common.Contracts.EventSourcing;
using Common.Contracts.Finance;
using Common.Contracts.Image;
using Common.Contracts.Notification;
using Common.Contracts.Tag;
using MassTransit;
using ProcessingService.StateMachines.RestoreStateMachine;

namespace ProcessingService.Activities.Restore;

public class ItemsResetActivity : IStateMachineActivity<RestoreState, SendStopFinishService>
{
    private readonly IPublishEndpoint _publishEndpoint;
    public ItemsResetActivity(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<RestoreState, SendStopFinishService> context, IBehavior<RestoreState, SendStopFinishService> next)
    {
        //посылаем сообщения на удаление всех записей из соответствующих сервисов
        await _publishEndpoint.Publish(new FinanceReset
        {
            CorrelationId = context.Saga.CorrelationId,
            CallBackType = "Common.Contracts.Processing.ResetItems"
        });
        await _publishEndpoint.Publish(new BidReset
        {
            CorrelationId = context.Saga.CorrelationId,
            CallBackType = "Common.Contracts.Processing.ResetItems"
        });
        await _publishEndpoint.Publish(new ImageReset
        {
            CorrelationId = context.Saga.CorrelationId,
            CallBackType = "Common.Contracts.Processing.ResetItems"
        });
        await _publishEndpoint.Publish(new AuctionReset
        {
            CorrelationId = context.Saga.CorrelationId,
            CallBackType = "Common.Contracts.Processing.ResetItems"
        });
        await _publishEndpoint.Publish(new NotificationReset
        {
            CorrelationId = context.Saga.CorrelationId,
            CallBackType = "Common.Contracts.Processing.ResetItems"
        });
        await _publishEndpoint.Publish(new CommunicationReset
        {
            CorrelationId = context.Saga.CorrelationId,
            CallBackType = "Common.Contracts.Processing.ResetItems"
        });
        await _publishEndpoint.Publish(new TagReset
        {
            CorrelationId = context.Saga.CorrelationId,
            CallBackType = "Common.Contracts.Processing.ResetItems"
        });

        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<RestoreState, SendStopFinishService, TException> context, IBehavior<RestoreState, SendStopFinishService> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction");
    }
}