using System.Reflection;
using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Bid;
using Common.Contracts.Finance;
using Common.Contracts.Image;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using Common.Utils;
using Common.Utils.Extentions;
using EventSourcingService.Data;
using EventSourcingService.Entities;
using EventSourcingService.Services;
using MassTransit;

namespace SearchService.Consumers;

public class SetSnapShotConsumer : IConsumer<DataForProcessingServicesList<string>>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _context;
    private static readonly AwaitLocker _locker = new AwaitLocker();
    private readonly IConfiguration _configuration;
    private readonly RestoreImageService _restoreImageService;

    public SetSnapShotConsumer(IPublishEndpoint publishEndpoint, IConfiguration configuration,
        EventSourcingDbContext context, RestoreImageService restoreImageService)
    {
        _publishEndpoint = publishEndpoint;
        _context = context;
        _configuration = configuration;
        _restoreImageService = restoreImageService;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<string>> consumeContext)
    {
        await _locker.LockAsync(async () =>
        {
            var listItems = new DataForProcessingServicesList
            {
                DataObjects = new List<DataForProcessingService>()
            };

            foreach (var item in consumeContext.Message.DataObjects)
            {
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
                            var _image = await RestoreImages(item, consumeContext.Message);
                            if (!_image) return;
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

    private async Task<bool> RestoreImages(DataForProcessingService imageItem, DataForProcessingServicesList<string> message)
    {
        //это не разбитое на части изображение, ничего не делаем
        if (imageItem.MessagePartCounts == 1) return true;
        var typedItem_ = JsonSerializer.Deserialize<ImageDTO>(imageItem.Data);
        imageItem.Data = typedItem_.Image;
        var image_ = _restoreImageService.GetImageString(imageItem);

        //это разбитое на части изображение, собираем
        if (string.IsNullOrEmpty(image_))
        {
            //не все части изображения собраны, возвращаем ответ для уменьшения счетчика необработанных изображений
            var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(message.CallBackType);
            sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, message.CorrelationId);
            sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, new DataForProcessingServicesList
            {
                DataObjects = new List<DataForProcessingService>()
            });
            sendObject.GetType().GetProperty("AllItemsCount").SetValue(sendObject, -2);
            await _publishEndpoint.Publish(sendObject);
            return false;
        }
        imageItem.Data = JsonSerializer.Serialize(new ImageDTO
        {
            AuctionId = typedItem_.AuctionId,
            Image = image_
        });
        return true;
    }
}
