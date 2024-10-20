using Common.Contracts;
using Confluent.Kafka;
using EventSourcingService.Consumers.Processing;
using EventSourcingService.Data;
using MassTransit;
using MassTransit.KafkaIntegration;

namespace EventSourcingService.Consumers;

public class EventSourcingEventConsumer : IConsumer<BaseStateContract>
{
    private readonly ILogger<EventSourcingEventConsumer> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _context;

    public EventSourcingEventConsumer(ILogger<EventSourcingEventConsumer> logger,
        IPublishEndpoint publishEndpoint, EventSourcingDbContext context)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
        _context = context;
    }

    public async Task Consume(ConsumeContext<BaseStateContract> context)
    {
        var ctx = context.ReceiveContext as KafkaReceiveContext<Ignore, BaseStateContract>;
        //_logger.LogInformation($"Message: {context.Message.Data}, Offset: {ctx?.Offset}");
        switch (context.Message.Type)
        {
            case nameof(UpdateAuctionStateContract):
                var updateProcessing = new AuctionUpdateProcessing(_publishEndpoint, _context);
                await updateProcessing.ProcessingUpdateAuctionState(context);
                break;
            case nameof(CommitAuctionUpdatingContract):
                var updateProcessing2 = new AuctionUpdateProcessing(_publishEndpoint, _context);
                await updateProcessing2.ProcessingCommitUpdateAuctionEvent(context);
                break;
            case nameof(CreateAuctionStateContract):
                var createProcessing = new AuctionCreateProcessing(_publishEndpoint, _context);
                await createProcessing.ProcessingCreateAuctionState(context);
                break;
            case nameof(CommitAuctionCreatingContract):
                var createProcessing2 = new AuctionCreateProcessing(_publishEndpoint, _context);
                await createProcessing2.ProcessingCommitCreateAuctionEvent(context);
                break;
            case nameof(DeleteAuctionStateContract):
                var deleteProcessing = new AuctionDeleteProcessing(_publishEndpoint, _context);
                await deleteProcessing.ProcessingDeleteAuctionState(context);
                break;
            case nameof(CommitAuctionDeletingContract):
                var deleteProcessing2 = new AuctionDeleteProcessing(_publishEndpoint, _context);
                await deleteProcessing2.ProcessingCommitDeleteAuctionEvent(context);
                break;
            case nameof(FinishAuctionStateContract):
                var finishProcessing = new AuctionFinishProcessing(_publishEndpoint, _context);
                await finishProcessing.ProcessingFinishAuctionState(context);
                break;
            case nameof(CommitAuctionFinishingContract):
                var finishProcessing2 = new AuctionFinishProcessing(_publishEndpoint, _context);
                await finishProcessing2.ProcessingCommitFinishAuctionEvent(context);
                break;
            case nameof(BidPlacedStateContract):
                var bidplacedProcessing = new BidPlacedProcessing(_publishEndpoint, _context);
                await bidplacedProcessing.ProcessingBidPlacedState(context);
                break;
            case nameof(CommitBidPlacingContract):
                var bidplacedProcessing2 = new BidPlacedProcessing(_publishEndpoint, _context);
                await bidplacedProcessing2.ProcessingCommitBidPlacedEvent(context);
                break;
            case nameof(CommitBidErrorContract):
                var commitError = new CommitErrorProcessing(_publishEndpoint, _context);
                await commitError.ProcessingCommitErrorEvent(context);
                break;
            default:
                break;
        }
    }
}