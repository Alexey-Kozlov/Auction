using System.Reflection;
using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Bid;
using Common.Contracts.EventSourcing;
using Common.Contracts.Finance;
using Common.Contracts.Image;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using Common.Utils;
using EventSourcingService.Data;
using EventSourcingService.Entities;
using MassTransit;

namespace SearchService.Consumers;

public class SetSnapShotConsumer : IConsumer<DataForProcessingServicesList<string>>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _context;
    private static readonly AwaitLocker _locker = new AwaitLocker();
    private readonly IConfiguration _configuration;

    public SetSnapShotConsumer(IPublishEndpoint publishEndpoint, IConfiguration configuration,
        EventSourcingDbContext context)
    {
        _publishEndpoint = publishEndpoint;
        _context = context;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<string>> consumeContext)
    {
        await _locker.LockAsync(async () =>
        {
            var i = 0;
            var listItems = new DataForProcessingServicesList
            {
                DataObjects = new List<DataForProcessingService>()
            };
            foreach (var item in consumeContext.Message.DataObjects)
            {
                i++;
                var document = JsonDocument.Parse(item.Data);
                var jsonElement = new JsonElement();
                Guid? auctionId = null;
                string userLogin = "";
                listItems.DataObjects.Add
                (
                    new DataForProcessingService
                    {
                        DataType = "",
                        Data = "",
                        CRUD = CRUD.Create
                    }
                );

                try
                {
                    //получаем пользователя - инициатора события
                    switch (item.DataType)
                    {
                        case nameof(BidItem):
                            document.RootElement.TryGetProperty("Bidder", out jsonElement);
                            break;
                        case nameof(AuctionItem):
                            document.RootElement.TryGetProperty("Seller", out jsonElement);
                            break;
                        case nameof(FinanceItem):
                            document.RootElement.TryGetProperty("UserLogin", out jsonElement);
                            break;
                        case nameof(NotifyItem):
                            document.RootElement.TryGetProperty("UserLogin", out jsonElement);
                            break;
                        case nameof(ImageItem):
                            break;
                        default:
                            break;
                    }
                    jsonElement.TryGetsString(out userLogin);
                    //получаем Id аукциона
                    if (document.RootElement.TryGetProperty("AuctionId", out jsonElement))
                    {
                        auctionId = jsonElement.GetGuid();
                    }
                }
                catch { }
                _context.EventsLogs.Add(new EventsLog
                {
                    CorrelationId = Guid.NewGuid(),
                    CreateAt = DateTime.Parse(consumeContext.Message.Props),
                    Commited = true,
                    EventData = JsonDocument.Parse(item.Data),
                    SnapShotId = consumeContext.Message.CorrelationId,
                    EntityType = item.DataType,
                    UserLogin = userLogin,
                    AuctionId = auctionId,
                    Command = Command.MakeSnapShot
                });
            }
            await _context.SaveChangesAsync();
            var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(consumeContext.Message.CallBackType);
            sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, consumeContext.Message.CorrelationId);
            sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, listItems);
            sendObject.GetType().GetProperty("AllItemsCount").SetValue(sendObject, -1);

            await _publishEndpoint.Publish(sendObject);
        });
    }
}
