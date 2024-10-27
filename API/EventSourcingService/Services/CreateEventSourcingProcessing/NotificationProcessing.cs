using System.Text.Json;
using Common.Contracts;
using EventSourcingService.Data;
using EventSourcingService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Services.CreateEventSourcingProcessing;

public class NotificationProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _context;

    public NotificationProcessing(IPublishEndpoint publishEndpoint, EventSourcingDbContext context)
    {
        _publishEndpoint = publishEndpoint;
        _context = context;
    }
    public async Task Processing(ConsumeContext<BaseStateContract> context)
    {
        switch (context.Message.EntityType)
        {
            //ProcessingService -> CreateActivity -> NotificationActivity                         
            case nameof(AuctionCreatingNotification):
                await _publishEndpoint.Publish(new AuctionCreateESNotification(context.Message.CorrelationId));
                break;
        }
    }
}