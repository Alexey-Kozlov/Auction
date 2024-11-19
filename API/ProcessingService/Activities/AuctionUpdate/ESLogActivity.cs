using Common.Contracts.Auction;
using Common.Contracts.Finance;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.UpdateAuctionStateMachine;

namespace ProcessingService.Activities.AuctionUpdate;

public class ESLogActivity : IStateMachineActivity<UpdateAuctionState, RequestAuctionUpdate>
{
    private readonly SendEventToES _sendEventToES;
    private readonly IConfiguration _config;
    public ESLogActivity(SendEventToES sendEventToES, IConfiguration config)
    {
        _sendEventToES = sendEventToES;
        _config = config;
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(BehaviorContext<UpdateAuctionState, RequestAuctionUpdate> context, IBehavior<UpdateAuctionState, RequestAuctionUpdate> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new AuctionItem
            {
                Title = context.Message.Title,
                AuctionId = context.Message.AuctionId,
                Description = context.Message.Description,
                Properties = context.Message.Properties,
                Seller = context.Message.UserLogin,
                AuctionEnd = context.Message.AuctionEnd
            },
            nameof(AuctionItem),
            "Common.Contracts.Processing.ESLog_AuctionUpdated",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            Command.AuctionUpdate,
            context.Saga.AuctionId);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<UpdateAuctionState, RequestAuctionUpdate, TException> context, IBehavior<UpdateAuctionState, RequestAuctionUpdate> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction");
    }
}