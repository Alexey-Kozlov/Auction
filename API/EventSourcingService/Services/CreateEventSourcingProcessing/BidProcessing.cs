using System.Text.Json;
using Common.Contracts;
using EventSourcingService.Data;
using EventSourcingService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Services.CreateEventSourcingProcessing;

public class BidProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;

    public BidProcessing(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }
    public async Task Processing(ConsumeContext<BaseStateContract> context)
    {
        switch (context.Message.EntityType)
        {
            //ProcessingService -> UpdatedAuction -> BidActivity
            case nameof(AuctionUpdatingBid):
                await _publishEndpoint.Publish(new AuctionUpdateESBid(context.Message.CorrelationId));
                break;
        }
    }
}