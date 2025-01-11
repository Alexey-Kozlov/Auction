using System.Reflection;
using System.Text.Json;
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

    public AuctionCreateProcessing(IPublishEndpoint publishEndpoint, EventSourcingDbContext dbContext,
        IConfiguration configuration, RestoreImageService restoreImageService)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _configuration = configuration;
        _restoreImageService = restoreImageService;
    }

    public async Task ProcessESLog(ConsumeContext<ESContract> context)
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
        }
        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
        sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, listItems);
        await _publishEndpoint.Publish(sendObject);
    }

    private string ProcessImage(AuctionImageDTO imageDto)
    {
        //сборка изображения из нескольких частей
        var partImage = JsonSerializer.Deserialize<DataForProcessingService>(imageDto.Image);
        return _restoreImageService.GetImageString(partImage);
    }

}