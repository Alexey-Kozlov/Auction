using Common.Contracts;
using EventSourcingService.Services;
using EventSourcingService.Services.CreateEventSourcingProcessing;
using MassTransit;

namespace EventSourcingService.Consumers;

public class CreateEventSourcingItemConsumer : IConsumer<BaseStateContract>
{
    private readonly ILogger<CreateEventSourcingItemConsumer> _logger;
    private readonly InsertItemToEventSourcing _insertItemToEventSourcing;
    private readonly NotificationProcessing _notificationProcessing;
    private readonly SearchProcessing _searchProcessing;
    private readonly BidProcessing _bidProcessing;
    private readonly MainProcessing _mainProcessing;

    public CreateEventSourcingItemConsumer(ILogger<CreateEventSourcingItemConsumer> logger,
        InsertItemToEventSourcing insertItemToEventSourcing, NotificationProcessing notificationProcessing,
        SearchProcessing searchProcessing, BidProcessing bidProcessing, MainProcessing mainProcessing)
    {
        _logger = logger;
        _insertItemToEventSourcing = insertItemToEventSourcing;
        _notificationProcessing = notificationProcessing;
        _searchProcessing = searchProcessing;
        _bidProcessing = bidProcessing;
        _mainProcessing = mainProcessing;
    }

    public async Task Consume(ConsumeContext<BaseStateContract> context)
    {
        //var ctx = context.ReceiveContext as KafkaReceiveContext<Ignore, BaseStateContract>;
        //сохраняем новое сообщение в БД EventSourcing
        await _insertItemToEventSourcing.Processing(context.Message);
        //рассылаем сообщения для продолжения (RabbitMQ)
        switch (context.Message.ServiceName)
        {
            case "BiddingService":
                await _bidProcessing.Processing(context);
                break;
            case "SearchService":
                await _searchProcessing.Processing(context);
                break;
            case "NotificationService":
                await _notificationProcessing.Processing(context);
                break;
            case "ProcessingService":
                await _mainProcessing.Processing(context);
                break;
            default:
                break;
        }
    }
}