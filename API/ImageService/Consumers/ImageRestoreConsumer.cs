using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using AutoMapper;
using Common.Contracts.Image;
using Common.Contracts.Processing;
using Common.Utils;
using ImageService.Data;
using ImageService.Services;
using MassTransit;

namespace ImageService.Consumers;

public class ImageRestoreConsumer : IConsumer<DataForProcessingServicesList<ImageDTO>>
{
    private readonly ImageDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly RestoreImageService _restoreImageService;


    public ImageRestoreConsumer(ImageDbContext context, IPublishEndpoint publishEndpoint,
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
        var image = _restoreImageService.GetImageString(context.Message.DataObjects[0]);
        //если вернулась пустая строка - не все части изображения собраны, ждем остальных частей
        if (string.IsNullOrEmpty(image)) return;
        //вернулась не пустая строка - изображение собрано, формируем изображение и пишем в БД
        var typedItem = JsonSerializer.Deserialize<ImageDTO>(context.Message.DataObjects[0].Data);
        typedItem.Image = image;
        await _context.Images.AddAsync(_mapper.Map<ImageItem>(typedItem));
        await _context.SaveChangesAsync();
        //возвращаем сообщение - что обработано изображение
        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
        sendObject.GetType().GetProperty("BatchCounter").SetValue(sendObject, context.Message.DataObjects.Count());
        await _publishEndpoint.Publish(sendObject);

    }
}
