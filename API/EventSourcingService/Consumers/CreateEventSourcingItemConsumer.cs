using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using EventSourcingService.Services;
using MassTransit;

namespace EventSourcingService.Consumers;

public class CreateEventSourcingItemConsumer : IConsumer<ESContract>
{
    private readonly AuctionDeleteProcessing _auctionDeleteProcessing;
    private readonly AuctionCreateProcessing _auctionCreateProcessing;
    private readonly AuctionUpdateProcessing _auctionUpdateProcessing;
    private readonly ESLogCommitProcessing _eSLogCommitProcessing;
    private readonly ElkIndexProcessing _elkIndexProcessing;
    private readonly FinanceCreateProcessing _financeCreateProcessing;
    private readonly BidPlaceProcessing _bidPlaceProcessing;
    private readonly EditNotificationProcessing _editNotificationProcessing;
    private readonly RestoreSnapShotProcessing _restoreSnapShotProcessing;
    private readonly CommunicationCreateProcessing _communicationCreateProcessing;
    private readonly CommunicationDeleteProcessing _communicationDeleteProcessing;
    private readonly CommunicationUpdateProcessing _communicationUpdateProcessing;
    private readonly TagCreateProcessing _tagCreateProcessing;
    private readonly TagDeleteProcessing _tagDeleteProcessing;

    public CreateEventSourcingItemConsumer(AuctionDeleteProcessing auctionDeleteProcessing,
        ESLogCommitProcessing eSLogCommitProcessing,
        AuctionCreateProcessing auctionCreateProcessing,
        AuctionUpdateProcessing auctionUpdateProcessing,
        ElkIndexProcessing elkIndexProcessing,
        FinanceCreateProcessing financeCreateProcessing,
        BidPlaceProcessing bidPlaceProcessing,
        EditNotificationProcessing editNotificationProcessing,
        RestoreSnapShotProcessing restoreSnapShotProcessing,
        CommunicationCreateProcessing communicationCreateProcessing,
        CommunicationDeleteProcessing communicationDeleteProcessing,
        CommunicationUpdateProcessing communicationUpdateProcessing,
        TagCreateProcessing tagCreateProcessing,
        TagDeleteProcessing tagDeleteProcessing)
    {
        _auctionDeleteProcessing = auctionDeleteProcessing;
        _eSLogCommitProcessing = eSLogCommitProcessing;
        _auctionCreateProcessing = auctionCreateProcessing;
        _auctionUpdateProcessing = auctionUpdateProcessing;
        _elkIndexProcessing = elkIndexProcessing;
        _financeCreateProcessing = financeCreateProcessing;
        _bidPlaceProcessing = bidPlaceProcessing;
        _editNotificationProcessing = editNotificationProcessing;
        _restoreSnapShotProcessing = restoreSnapShotProcessing;
        _communicationCreateProcessing = communicationCreateProcessing;
        _communicationDeleteProcessing = communicationDeleteProcessing;
        _communicationUpdateProcessing = communicationUpdateProcessing;
        _tagCreateProcessing = tagCreateProcessing;
        _tagDeleteProcessing = tagDeleteProcessing;
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
            //вызываем соответствующий сервис для обработки поступившего по Кафке сообщения
            switch (context.Message.Command)
            {
                case Command.AuctionDelete:
                    await _auctionDeleteProcessing.ProcessESLog(context);
                    break;
                case Command.AuctionCreate:
                    await _auctionCreateProcessing.ProcessESLog(context);
                    break;
                case Command.AuctionUpdate:
                    await _auctionUpdateProcessing.ProcessESLog(context);
                    break;
                case Command.IndexELK:
                    await _elkIndexProcessing.ProcessElkIndex(context);
                    break;
                case Command.FinanceCreate:
                    await _financeCreateProcessing.ProcessESLog(context);
                    break;
                case Command.PlaceBid:
                    await _bidPlaceProcessing.ProcessESLog(context);
                    break;
                case Command.EditNotification:
                    await _editNotificationProcessing.ProcessESLog(context);
                    break;
                case Command.RestoreSnapShot:
                    await _restoreSnapShotProcessing.ProcessESLog(context);
                    break;
                case Command.CommunicationCreate:
                    await _communicationCreateProcessing.ProcessESLog(context);
                    break;
                case Command.CommunicationDelete:
                    await _communicationDeleteProcessing.ProcessESLog(context);
                    break;
                case Command.CommunicationUpdate:
                    await _communicationUpdateProcessing.ProcessESLog(context);
                    break;
                case Command.TagCreate:
                    await _tagCreateProcessing.ProcessESLog(context);
                    break;
                case Command.TagDelete:
                    await _tagDeleteProcessing.ProcessESLog(context);
                    break;
                default:
                    break;
            }
        }
    }
}