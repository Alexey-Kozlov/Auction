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
    private readonly ElkIndexProcessing _elkIndexProcessing;

    public CreateEventSourcingItemConsumer(AuctionDeleteProcessing auctionDeleteProcessing,
        ESLogCommitProcessing eSLogCommitProcessing,
        AuctionCreateProcessing auctionCreateProcessing,
        ElkIndexProcessing elkIndexProcessing)
    {
        _auctionDeleteProcessing = auctionDeleteProcessing;
        _eSLogCommitProcessing = eSLogCommitProcessing;
        _auctionCreateProcessing = auctionCreateProcessing;
        _elkIndexProcessing = elkIndexProcessing;
    }

    public async Task Consume(ConsumeContext<ESContract> context)
    {
        //var ctx = context.ReceiveContext as KafkaReceiveContext<Ignore, ESContract>;
        if (context.Message.EntityType == nameof(CommitESOperation))
        {
            await _eSLogCommitProcessing.CommitESLog(context);
        }
        else
        {
            //рассылаем сообщения для продолжения (RabbitMQ)
            switch (context.Message.Command)
            {
                case Command.AuctionDelete:
                    await _auctionDeleteProcessing.ProcessESLog(context);
                    break;
                case Command.AuctionCreate:
                    await _auctionCreateProcessing.ProcessESLog(context);
                    break;
                case Command.IndexELK:
                    await _elkIndexProcessing.ProcessElkIndex(context);
                    break;
                default:
                    break;
            }
        }
    }
}