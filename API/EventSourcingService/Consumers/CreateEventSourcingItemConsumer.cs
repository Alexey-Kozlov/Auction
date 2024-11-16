using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using EventSourcingService.Services;
using MassTransit;

namespace EventSourcingService.Consumers;

public class CreateEventSourcingItemConsumer : IConsumer<ESContract>
{
    private readonly AuctionDeleteProcessing _auctionDeleteProcessing;
    private readonly AuctionCreateProcessing _auctionCreateProcessing;
    private readonly ESLogCommitProcessing _eSLogCommitProcessing;

    public CreateEventSourcingItemConsumer(AuctionDeleteProcessing auctionDeleteProcessing,
        ESLogCommitProcessing eSLogCommitProcessing,
        AuctionCreateProcessing auctionCreateProcessing)
    {
        _auctionDeleteProcessing = auctionDeleteProcessing;
        _eSLogCommitProcessing = eSLogCommitProcessing;
        _auctionCreateProcessing = auctionCreateProcessing;
    }

    public async Task Consume(ConsumeContext<ESContract> context)
    {
        //var ctx = context.ReceiveContext as KafkaReceiveContext<Ignore, ESContract>;
        if (context.Message.EntityType == nameof(CommitESOperation))
        {
            await _eSLogCommitProcessing.CommitESLog(context);
        }
        //рассылаем сообщения для продолжения (RabbitMQ)
        switch (context.Message.Command)
        {
            case Command.AuctionDelete:
                await _auctionDeleteProcessing.ProcessESLog(context);
                break;
            case Command.AuctionCreate:
                await _auctionCreateProcessing.ProcessESLog(context);
                break;
            default:
                break;
        }
    }
}