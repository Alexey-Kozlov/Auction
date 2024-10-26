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
    public async Task Processing(ConsumeContext<BaseStateContract> context)
    {
        switch (context.Message.EntityType)
        {
            //ProcessingService -> UpdatedActivity -> SearchActivity                         
            case nameof(AuctionUpdatingSearch):
                await _publishEndpoint.Publish(new AuctionUpdateESSearch(context.Message.CorrelationId));
                break;
        }
    }
}