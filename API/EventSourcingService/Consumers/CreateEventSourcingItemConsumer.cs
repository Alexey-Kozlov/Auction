using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using EventSourcingService.Services.CreateEventSourcingProcessing;
using MassTransit;

namespace EventSourcingService.Consumers;

public class CreateEventSourcingItemConsumer : IConsumer<ESContract>
{
    private readonly AuctionDeleteProcessing _auctionDeleteProcessing;

    public CreateEventSourcingItemConsumer(AuctionDeleteProcessing auctionDeleteProcessing)
    {
        _auctionDeleteProcessing = auctionDeleteProcessing;
    }

    public async Task Consume(ConsumeContext<ESContract> context)
    {
        //var ctx = context.ReceiveContext as KafkaReceiveContext<Ignore, ESContract>;
        //рассылаем сообщения для продолжения (RabbitMQ)
        switch (context.Message.OperationType)
        {
            case OperationType.AuctionDelete:
                await _auctionDeleteProcessing.Processing(context);
                break;
            // case nameof(AuctionItem):
            //     await _searchProcessing.Processing(context);
            //     break;
            // case nameof(NotifyItem):
            //     await _notificationProcessing.Processing(context);
            //     break;
            // case nameof(CommitESOperation):
            //     await _mainProcessing.Processing(context);
            //     break;
            // case nameof(FinanceItem):
            //     await _financeProcessing.Processing(context);
            //     break;
            default:
                break;
        }
    }
}