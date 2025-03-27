using System.Reflection;
using System.Text.Json;
using AuctionService.Metrics;
using Common.Contracts.Auction;
using Common.Contracts.EventSourcing;
using Common.Contracts.Processing;
using EventSourcingService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Services;

public class AuctionCreateProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly RestoreImageService _restoreImageService;
    private readonly AuctionMetrics _auctionMetrics;
    private readonly CheckAuctionFinished _checkAuctionFinished;

    public AuctionCreateProcessing(IPublishEndpoint publishEndpoint, EventSourcingDbContext dbContext,
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
            var imageDto = new AuctionImageDTO
            {
                Image = "",
                IsImageSplitted = false,
                UsingImage = false
            };
            //если есть изображение
            if (!string.IsNullOrEmpty(context.Message.Image))
            {
                //если изображение было разбито на части - собираем изображение
                imageDto = JsonSerializer.Deserialize<AuctionImageDTO>(context.Message.Image);
                if (imageDto.IsImageSplitted)
                {
                    imageDto.Image = ProcessImage(imageDto);
                    //если вернулась пустая строка - не все части изображения собраны
                    if (string.IsNullOrEmpty(imageDto.Image))
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
                    //все части изображения собраны - продолжаем обработку изображения
                }
            }
            //В процедуре Postgres делаем:
            //- запись в ES лог о создании аукциона
            //- записи в ES лог о создании уведомления для пользователя, создавшего аукцион
            //Формирование списка корректирующих записей:
            //- запись созданного аукциона - для добавления в сервис SearchService
            //- запись созданного уведомления - для добавления в сервис NotificationService

            var result = await _dbContext.auction_create(
                context.Message.CorrelationId,
                context.Message.AuctionId ?? Guid.NewGuid(),
                context.Message.EventData,
                context.Message.UserLogin,
                JsonSerializer.Serialize(imageDto)
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
                        auction.AuctionEnd, CRUD.Create);
                }
            }
            //считаем в метриках - создание аукциона
            _auctionMetrics.AddAuction();

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
            messageObject.GetType().GetProperty("Message").SetValue(messageObject, e.Message);
            messageObject.GetType().GetProperty("ExceptionMessage").SetValue(messageObject, e.StackTrace);
            messageObject.GetType().GetProperty("ServiceName").SetValue(messageObject, "EventSourcingService_AuctionCreate");
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

    private string ProcessImage(AuctionImageDTO imageDto)
    {
        //сборка изображения из нескольких частей
        var partImage = JsonSerializer.Deserialize<DataForProcessingService>(imageDto.Image);
        return _restoreImageService.GetImageString(partImage);
    }

}