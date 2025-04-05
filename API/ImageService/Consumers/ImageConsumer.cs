using System.Reflection;
using AutoMapper;
using Common.Contracts.Auction;
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
    private static readonly AwaitLocker _locker = new AwaitLocker();

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
        await _locker.LockAsync(async () =>
        {
            var callBack = context.Message.CallBackType.Split(",").Length > 1 ?
                context.Message.CallBackType.Split(",")[0] :
                context.Message.CallBackType;
            try
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
                        await _publishEndpoint.Publish(new AuctionUpdateFinalize
                        {
                            CorrelationId = correlationId
                        });
                        var sendObject_dop = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType.Split(",")[1]);
                        sendObject_dop.GetType().GetProperty("CorrelationId").SetValue(sendObject_dop, correlationId);
                        await _publishEndpoint.Publish(sendObject_dop);
                        return;
                    }
                    imageItem.Data = image;
                }
                var typedItem = new ImageDTO
                {
                    AuctionId = crudItem.MessagePartId,
                    Image = imageItem.Data
                };
                typedItem.CorrelationId = correlationId;
                switch (crudItem.CRUD)
                {
                    case CRUD.Delete:
                        var item = await _context.Images.FirstOrDefaultAsync(p =>
                            p.AuctionId == crudItem.MessagePartId && p.Commited);
                        if (item != null)
                        {
                            item.CorrelationId = correlationId;
                            _context.Images.Update(item);
                        }
                        break;
                    case CRUD.Create:
                        typedItem.Id = Guid.NewGuid();
                        typedItem.Commited = false;
                        await _context.AddAsync(_mapper.Map<ImageItem>(typedItem));
                        break;
                    case CRUD.Update:
                        var item2 = await _context.Images.FirstOrDefaultAsync(p =>
                            p.AuctionId == crudItem.MessagePartId && p.Commited);
                        if (item2 != null)
                        {
                            item2.CorrelationId = correlationId;
                            _context.Images.Update(item2);
                        }
                        typedItem.Id = Guid.NewGuid();
                        typedItem.Commited = false;
                        await _context.AddAsync(_mapper.Map<ImageItem>(typedItem));
                        break;
                }
                //операция над изображением выполнена, продолжаем обработку в Saga
                await _context.SaveChangesAsync();
                var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                            _configuration["CommonAssembly"]).CreateInstance(callBack);
                sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
                await _publishEndpoint.Publish(sendObject);
            }
            catch (Exception e)
            {
                //ошибки, в т.ч. штатные
                var messageObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                    _configuration["CommonAssembly"]).CreateInstance(callBack);
                messageObject.GetType().GetProperty("CorrelationId").SetValue(messageObject, context.Message.CorrelationId);
                messageObject.GetType().GetProperty("Message").SetValue(messageObject, e.Message);
                messageObject.GetType().GetProperty("ExceptionMessage").SetValue(messageObject, e.StackTrace);
                messageObject.GetType().GetProperty("ServiceName").SetValue(messageObject, "ImageService");
                messageObject.GetType().GetProperty("UserLogin").SetValue(messageObject, "");
                messageObject.GetType().GetProperty("AuctionId").SetValue(messageObject, null);
                messageObject.GetType().GetProperty("IsError").SetValue(messageObject, true);
                var faultType = typeof(FaultMessage<>);
                var typeParams = new Type[] { messageObject.GetType() };
                var faultObjectType = faultType.MakeGenericType(typeParams);
                var faultObject = Activator.CreateInstance(faultObjectType, new object[] { messageObject });

                await _publishEndpoint.Publish(faultObject.GetType().GetMethod("CastItem").Invoke(faultObject, null));
            }
        });
    }
}
