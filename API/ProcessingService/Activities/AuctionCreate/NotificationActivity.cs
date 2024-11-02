using Common.Contracts;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.CreateAuctionStateMachine;

namespace ProcessingService.Activities.AuctionCreate;

public class NotificationActivity : IStateMachineActivity<CreateAuctionState, AuctionCreatedSearch>
{
    private readonly SendEventToES _sendEventToES;
    private readonly IConfiguration _config;
    public NotificationActivity(SendEventToES sendEventToES, IConfiguration config)
    {
        _sendEventToES = sendEventToES;
        _config = config;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<CreateAuctionState, AuctionCreatedSearch> context, IBehavior<CreateAuctionState, AuctionCreatedSearch> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new NotifyItem
            {
                AuctionId = context.Saga.AuctionId,
                UserLogin = context.Saga.UserLogin
            },
            nameof(NotifyItem),
            _config["ServicesName:NotificationService"],
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            OperationType.Insert,
            context.Saga.AuctionId);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<CreateAuctionState, AuctionCreatedSearch, TException> context, IBehavior<CreateAuctionState, AuctionCreatedSearch> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-create");
    }
}