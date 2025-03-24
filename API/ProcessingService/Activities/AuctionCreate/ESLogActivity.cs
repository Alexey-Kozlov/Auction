using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Processing;
using Common.Utils;
using MassTransit;
using ProcessingService.StateMachines.CreateAuctionStateMachine;

namespace ProcessingService.Activities.AuctionCreate;

public class ESLogActivity : IStateMachineActivity<CreateAuctionState, RequestAuctionCreate>
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

    public async Task Execute(BehaviorContext<CreateAuctionState, RequestAuctionCreate> context, IBehavior<CreateAuctionState, RequestAuctionCreate> next)
    {
        await _sendEventToES.SendItemToEventSourcing(
            new AuctionItem
            {
                AuctionId = context.Saga.AuctionId,
                Title = context.Saga.Title,
                Properties = context.Saga.Properties,
                Description = context.Saga.Description,
                Seller = context.Saga.UserLogin,
                AuctionEnd = context.Saga.AuctionEnd,
                ReservePrice = context.Saga.ReservePrice,
                CreateAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            nameof(AuctionItem),
            "Common.Contracts.Processing.ESLogAuctionCreated",
            context.Message.CorrelationId,
            context.Saga.UserLogin,
            Command.AuctionCreate,
            JsonSerializer.Serialize(new AuctionImageDTO
            {
                Image = context.Message.Image,
                UsingImage = context.Message.UsingImage,
                IsImageSplitted = context.Message.IsImageSplitted
            }),
            context.Saga.AuctionId,
            false);
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<CreateAuctionState, RequestAuctionCreate, TException> context, IBehavior<CreateAuctionState, RequestAuctionCreate> next) where TException : Exception
    {
        return next.Faulted(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("request-auction-create");
    }
}