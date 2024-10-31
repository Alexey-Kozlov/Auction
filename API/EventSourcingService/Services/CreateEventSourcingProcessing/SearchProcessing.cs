using System.Text.Json;
using Common.Contracts;
using EventSourcingService.Data;
using EventSourcingService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Services.CreateEventSourcingProcessing;

public class SearchProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;

    public SearchProcessing(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }
    public async Task Processing(ConsumeContext<ESContract> context)
    {
        switch (context.Message.EntityType)
        {
            //ProcessingService -> Activities -> AuctionUpdate -> SearchActivity                         
            case nameof(AuctionUpdatingSearch):
                await _publishEndpoint.Publish(
                    JsonSerializer.Deserialize<AuctionUpdatingSearch>(context.Message.EventData)
                );
                break;
            //ProcessingService -> Activities -> AuctionCreate -> SearchActivity                         
            case nameof(AuctionCreatingSearch):
                await _publishEndpoint.Publish(
                    JsonSerializer.Deserialize<AuctionCreatingSearch>(context.Message.EventData)
                );
                break;
        }
    }
}