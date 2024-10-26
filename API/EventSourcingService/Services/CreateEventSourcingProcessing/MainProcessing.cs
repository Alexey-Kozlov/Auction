using Common.Contracts;
using EventSourcingService.Data;
using MassTransit;

namespace EventSourcingService.Services.CreateEventSourcingProcessing;

public class MainProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _context;

    public MainProcessing(IPublishEndpoint publishEndpoint, EventSourcingDbContext context)
    {
        _publishEndpoint = publishEndpoint;
        _context = context;
    }
    public async Task Processing(ConsumeContext<BaseStateContract> context)
    {
        switch (context.Message.EntityType)
        {
            //ProcessingService -> UpdatedActivity -> CommitActivity                         
            case nameof(CommitESOperation):
                await _publishEndpoint.Publish(new AuctionUpdateESCommit(context.Message.CorrelationId));
                break;
        }


    }
}