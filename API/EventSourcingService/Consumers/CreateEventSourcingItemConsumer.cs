using Common.Contracts;
using EventSourcingService.Services;
using EventSourcingService.Services.CreateEventSourcingProcessing;
using MassTransit;

namespace EventSourcingService.Consumers;

public class CreateEventSourcingItemConsumer : IConsumer<ESContract>
{
    private readonly ILogger<CreateEventSourcingItemConsumer> _logger;
    private readonly InsertItemToEventSourcing _insertItemToEventSourcing;
    private readonly NotificationProcessing _notificationProcessing;
    private readonly SearchProcessing _searchProcessing;
    private readonly BidProcessing _bidProcessing;
    private readonly MainProcessing _mainProcessing;
    private readonly FinanceProcessing _financeProcessing;
    public readonly IConfiguration _configuration;

    public CreateEventSourcingItemConsumer(ILogger<CreateEventSourcingItemConsumer> logger,
        InsertItemToEventSourcing insertItemToEventSourcing, NotificationProcessing notificationProcessing,
        SearchProcessing searchProcessing, BidProcessing bidProcessing, MainProcessing mainProcessing,
        IConfiguration configuration, FinanceProcessing financeProcessing)
    {
        _logger = logger;
        _insertItemToEventSourcing = insertItemToEventSourcing;
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
        //сохраняем новое сообщение в БД EventSourcing
        await _insertItemToEventSourcing.Processing(context.Message);
        //рассылаем сообщения для продолжения (RabbitMQ)
        switch (context.Message.ServiceName)
        {
            case var val when val == _configuration["ServicesName:BiddingService"]:
                await _bidProcessing.Processing(context);
                break;
            case var val when val == _configuration["ServicesName:SearchService"]:
                await _searchProcessing.Processing(context);
                break;
            case var val when val == _configuration["ServicesName:NotificationService"]:
                await _notificationProcessing.Processing(context);
                break;
            case var val when val == _configuration["ServicesName:ProcessingService"]:
                await _mainProcessing.Processing(context);
                break;
            case var val when val == _configuration["ServicesName:FinanceService"]:
                await _financeProcessing.Processing(context);
                break;
            default:
                break;
        }
    }
}