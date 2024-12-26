using System.Reflection;
using AutoMapper;
using Common.Contracts.Image;
using Common.Contracts.Processing;
using Common.Utils;
using ImageService.Data;
using ImageService.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ImageService.Consumers;

public class ImageConsumer : IConsumer<DataForProcessingServicesList<ImageDTO>>
{
    private readonly ImageDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly RestoreImageService _restoreImageService;

    public ImageConsumer(ImageDbContext context, IPublishEndpoint publishEndpoint,
        IConfiguration configuration, IMapper mapper, RestoreImageService restoreImageService)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
        _mapper = mapper;
        _restoreImageService = restoreImageService;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<ImageDTO>> context)
    {
        var correlationId = context.Message.CorrelationId;
        var crudItem = context.Message.DataObjects.First(p => p.Data == "CRUD");
        DataForProcessingService imageItem = new DataForProcessingService();
        if (crudItem.CRUD == CRUD.Create || crudItem.CRUD == CRUD.Update)
        {
            imageItem = context.Message.DataObjects.First(p => p.Data != "");
            var image = _restoreImageService.GetImageString(imageItem);
            if (string.IsNullOrEmpty(image))
            {
                //если вернули пустую строку - еще не все части изображения собраны,
                //возвращаем сообщение для финализирования Saga для данного потока.
                //Далее ждем, когда все части изображения будут собраны
                return;
            }
            imageItem.Data = image;
        }
        var typedItem = new ImageDTO
        {
            AuctionId = crudItem.MessagePartId,
            Image = imageItem.Data
        };
        switch (crudItem.CRUD)
        {
            case CRUD.Delete:
                var item = await _context.Images.FirstOrDefaultAsync(p => p.AuctionId == crudItem.MessagePartId);
                if (item != null)
                {
                    _context.Images.Remove(item);
                }
                else
                {
                    throw new Exception("Ошибка - не найдена запись изображения");
                }
                break;
            case CRUD.Create:
                await _context.AddAsync(_mapper.Map<ImageItem>(typedItem));
                break;
            case CRUD.Update:

                var item2 = await _context.Images.FirstOrDefaultAsync(p => p.AuctionId == crudItem.MessagePartId);
                if (item2 != null)
                {
                    _mapper.Map(typedItem, item2);
                    _context.Images.Update(item2);
                }
                else
                {
                    //если было обновление аукциона без изображения
                    await _context.AddAsync(_mapper.Map<ImageItem>(typedItem));
                }
                break;
        }
        //операция над изображением выполнена, продолжаем обработку в Saga
        await _context.SaveChangesAsync();
        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                    _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
        await _publishEndpoint.Publish(sendObject);
    }
}
