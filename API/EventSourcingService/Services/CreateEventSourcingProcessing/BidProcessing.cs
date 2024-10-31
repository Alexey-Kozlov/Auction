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
    public async Task Processing(ConsumeContext<ESContract> context)
    {
        switch (context.Message.EntityType)
        {
            //ProcessingService -> Activities -> AuctionUpdate -> BidActivity
            case nameof(AuctionUpdatingBid):
                //обновили ES, теперь посылаем сообщение на обновление в BiddingSDervice
                await _publishEndpoint.Publish(
                    JsonSerializer.Deserialize<AuctionUpdatingBid>(context.Message.EventData)
                );
                break;
            //ProcessingService -> Activities -> AuctionCreate -> BidActivity
            case nameof(AuctionCreatingBid):
                await _publishEndpoint.Publish(
                    JsonSerializer.Deserialize<AuctionCreatingBid>(context.Message.EventData)
                );
                break;
        }
    }
}