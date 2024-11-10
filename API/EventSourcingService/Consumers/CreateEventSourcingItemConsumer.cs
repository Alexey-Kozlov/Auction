using Common.Contracts;
using EventSourcingService.Services.CreateEventSourcingProcessing;
using MassTransit;

namespace EventSourcingService.Consumers;

public class CreateEventSourcingItemConsumer : IConsumer<ESContract>
{
    private readonly ILogger<CreateEventSourcingItemConsumer> _logger;
    private readonly NotificationProcessing _notificationProcessing;
    private readonly SearchProcessing _searchProcessing;
    private readonly BidProcessing _bidProcessing;
    private readonly MainProcessing _mainProcessing;
    private readonly FinanceProcessing _financeProcessing;
    public readonly IConfiguration _configuration;

    public CreateEventSourcingItemConsumer(ILogger<CreateEventSourcingItemConsumer> logger,
        NotificationProcessing notificationProcessing,
        SearchProcessing searchProcessing, BidProcessing bidProcessing, MainProcessing mainProcessing,
        IConfiguration configuration, FinanceProcessing financeProcessing)
    {
        _logger = logger;
        _notificationProcessing = notificationProcessing;
        _searchProcessing = searchProcessing;
        _bidProcessing = bidProcessing;
        _mainProcessing = mainProcessing;
        _configuration = configuration;
        _financeProcessing = financeProcessing;
    }

    public async Task Consume(ConsumeContext<ESContract> context)
    {
        //var ctx = context.ReceiveContext as KafkaReceiveContext<Ignore, ESContract>;
        //рассылаем сообщения для продолжения (RabbitMQ)
        switch (context.Message.EntityType)
        {
            case nameof(AuctionBidItem):
            case nameof(BidItem):
                await _bidProcessing.Processing(context);
                break;
            case nameof(AuctionItem):
                await _searchProcessing.Processing(context);
                break;
            case nameof(NotifyItem):
                await _notificationProcessing.Processing(context);
                break;
            case nameof(CommitESOperation):
                await _mainProcessing.Processing(context);
                break;
            case nameof(FinanceItem):
                await _financeProcessing.Processing(context);
                break;
            default:
                break;
        }
    }
}