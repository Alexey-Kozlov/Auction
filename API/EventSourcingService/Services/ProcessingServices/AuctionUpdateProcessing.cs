using System.Reflection;
using System.Text.Json;
using AuctionService.Metrics;
using Common.Contracts.Auction;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using Common.Utils.Logging;
using EventSourcingService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Services;

public class AuctionUpdateProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly RestoreImageService _restoreImageService;
    private readonly AuctionMetrics _auctionMetrics;
    private readonly CheckAuctionFinished _checkAuctionFinished;

    public AuctionUpdateProcessing(IPublishEndpoint publishEndpoint, EventSourcingDbContext dbContext,
        IConfiguration configuration, RestoreImageService restoreImageService, AuctionMetrics auctionMetrics,
        CheckAuctionFinished checkAuctionFinished)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _configuration = configuration;
        _restoreImageService = restoreImageService;
        _auctionMetrics = auctionMetrics;
        _checkAuctionFinished = checkAuctionFinished;
    }

    public async Task ProcessESLog(ConsumeContext<ESContract> context)
    {
        try
        {
            var imageDto = JsonSerializer.Deserialize<RequestAuctionUpdate>(context.Message.Image);
            var imageData = JsonSerializer.Deserialize<DataForProcessingService>(imageDto.Image);
            //если есть изображение
            var fullImage = imageData.Data;
            if (!string.IsNullOrEmpty(imageData.Data))
            {
                //если изображение было разбито на части - собираем изображение
                if (imageDto.IsImageSplitted)
                {
                    fullImage = _restoreImageService.GetImageString(imageData);
                    //если вернулась пустая строка - не все части изображения собраны, конец обработки этого сообщения
                    if (string.IsNullOrEmpty(fullImage))
                    {
                        var sendObject_ = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                                    _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
                        sendObject_.GetType().GetProperty("CorrelationId").SetValue(sendObject_, context.Message.CorrelationId);
                        sendObject_.GetType().GetProperty("DataItems").SetValue(sendObject_, new DataForProcessingServicesList
                        {
                            DataObjects = new List<DataForProcessingService>()
                        });
                        await _publishEndpoint.Publish(sendObject_);
                        return;
                    }
                }
            }
            //либо все части изображения собраны - продолжаем обработку с изображением,
            //либо это была запись аукциона без изображения
            //в любом случае продолжаем обработку
            var result = await _dbContext.auction_update(
                                context.Message.CorrelationId,
                                context.Message.AuctionId.Value,
                                context.Message.EventData,
                                context.Message.UserLogin,
                                Convert.FromBase64String(
                                    fullImage.Replace("data:image/jpeg;base64,", "")
                                    .Replace("data:image/bmp;base64,", "")
                                    .Replace("data:image/jpg;base64,", "")
                                    .Replace("data:image/png;base64,", "")),
                                imageDto.UsingImage
                                ).ToListAsync();
            //возвращаем список записей для изменения соответствующих БД в нужных сервисах
            var listItems = new DataForProcessingServicesList
            {
                DataObjects = new List<DataForProcessingService>()
            };
            foreach (var item in result)
            {
                listItems.DataObjects.Add
                (
                    new DataForProcessingService
                    {
                        DataType = item.entitytype,
                        Data = item.eventdata,
                        CRUD = (CRUD)item.crud
                    }
                );
                if (item.entitytype == "AuctionItem")
                {
                    var auction = JsonSerializer.Deserialize<AuctionItem>(item.eventdata);
                    await _checkAuctionFinished.UpdateFinishTasks(auction.AuctionId,
                        auction.AuctionEnd, CRUD.Update);
                }
            }
            _auctionMetrics.UpdateAuction();
            var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
            sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, listItems);
            await _publishEndpoint.Publish(sendObject);
        }
        catch (Exception e)
        {
            //ошибки прочие
            var messageObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            messageObject.GetType().GetProperty("CorrelationId").SetValue(messageObject, context.Message.CorrelationId);
            messageObject.GetType().GetProperty("ErrorMessage").SetValue(messageObject, GetErrorMessage.GetInnerException(e).Message);
            messageObject.GetType().GetProperty("ErrorExceptionMessage").SetValue(messageObject, e.StackTrace);
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "EventSourcingService_AuctionUpdate");
            messageObject.GetType().GetProperty("UserLogin").SetValue(messageObject, context.Message.UserLogin);
            messageObject.GetType().GetProperty("AuctionId").SetValue(messageObject, context.Message.AuctionId);
            messageObject.GetType().GetProperty("IsError").SetValue(messageObject, true);

            var faultType = typeof(FaultMessage<>);
            var typeParams = new Type[] { messageObject.GetType() };
            var faultObjectType = faultType.MakeGenericType(typeParams);

            var faultObject = Activator.CreateInstance(faultObjectType, new object[] { messageObject });
            await _publishEndpoint.Publish(faultObject.GetType().GetMethod("CastItem").Invoke(faultObject, null));
        }
    }
}