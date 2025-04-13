using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Bid;
using Common.Contracts.Finance;
using Common.Contracts.Image;
using Common.Contracts.Notification;
using Common.Contracts.Processing;
using Common.Utils;
using Common.Utils.Extentions;
using Common.Utils.Vault;
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
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<string>> context)
    {
        try
        {
            await _locker.LockAsync(async () =>
            {
                var imageFull = "";
                var listItems = new DataForProcessingServicesList
                {
                    DataObjects = new List<DataForProcessingService>()
                };

                foreach (var item in context.Message.DataObjects)
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
                    //получаем Id аукциона
                    if (document.RootElement.TryGetProperty("AuctionId", out jsonElement))
                    {
                        var _tmpGuid = "";
                        if (jsonElement.TryGetsString(out _tmpGuid))
                        {
                            if (!string.IsNullOrEmpty(_tmpGuid) && _tmpGuid.ToLower() != "null")
                            {
                                auctionId = Guid.Parse(_tmpGuid);
                            }

                        }
                    }
                    //обрабатываем сообщения по их типу
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
                            imageFull = await RestoreImages(item, context.Message);
                            //выходим если была обработана часть изображения
                            if (string.IsNullOrEmpty(imageFull)) return;
                            item.Data = JsonSerializer.Serialize(new
                            {
                                AuctionId = auctionId,
                                item.Id,
                                Commited = true,
                                context.Message.CorrelationId
                            });
                            break;
                        default:
                            break;
                    }
                    jsonElement.TryGetsString(out userLogin);


                    _context.EventsLogs.Add(new EventsLog
                    {
                        CorrelationId = context.Message.CorrelationId,
                        CreateAt = DateTime.Parse(context.Message.Props),
                        Commited = false,
                        EventData = JsonDocument.Parse(item.Data),
                        SnapShotId = context.Message.CorrelationId,
                        EntityType = item.DataType,
                        UserLogin = userLogin,
                        AuctionId = auctionId,
                        Command = Command.MakeSnapShot,
                        Image = string.IsNullOrEmpty(imageFull) ? null : Convert.FromBase64String(imageFull)
                    });
                }
                await _context.SaveChangesAsync();

                var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                    _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
                sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
                sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, listItems);
                sendObject.GetType().GetProperty("AllItemsCount").SetValue(sendObject, -1);
                await _publishEndpoint.Publish(sendObject);
            });
        }
        catch (Exception e)
        {
            //ошибки прочие
            var messageObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            messageObject.GetType().GetProperty("CorrelationId").SetValue(messageObject, context.Message.CorrelationId);
            messageObject.GetType().GetProperty("ErrorMessage").SetValue(messageObject, e.Message);
            messageObject.GetType().GetProperty("ErrorExceptionMessage").SetValue(messageObject, e.StackTrace);
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "EventSourcingService_SetSnapShot");
            messageObject.GetType().GetProperty("UserLogin").SetValue(messageObject, "");
            messageObject.GetType().GetProperty("AuctionId").SetValue(messageObject, null);
            messageObject.GetType().GetProperty("IsError").SetValue(messageObject, true);

            var faultType = typeof(FaultMessage<>);
            var typeParams = new Type[] { messageObject.GetType() };
            var faultObjectType = faultType.MakeGenericType(typeParams);

            var faultObject = Activator.CreateInstance(faultObjectType, new object[] { messageObject });
            await _publishEndpoint.Publish(faultObject.GetType().GetMethod("CastItem").Invoke(faultObject, null));
        }
    }

    private async Task<string> RestoreImages(DataForProcessingService imageItem,
        DataForProcessingServicesList<string> message)
    {
        //если imageItem.MessagePartCounts == 1 - это не разбитое на части изображение, ничего не делаем
        var typedItem_ = JsonSerializer.Deserialize<ImageDTO>(imageItem.Data);
        if (imageItem.MessagePartCounts == 1) return typedItem_.Image;
        //это разбитое на части изображение, собираем

        imageItem.Data = typedItem_.Image;
        var image_ = _restoreImageService.GetImageString(imageItem);


        if (string.IsNullOrEmpty(image_))
        {
            //не все части изображения собраны, возвращаем ответ для индикатора прогресса
            var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(message.CallBackType);
            sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, message.CorrelationId);
            sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, new DataForProcessingServicesList
            {
                DataObjects = new List<DataForProcessingService>()
            });
            sendObject.GetType().GetProperty("AllItemsCount").SetValue(sendObject, -2);
            await _publishEndpoint.Publish(sendObject);
            return null;
        }

        return image_;
    }
}
