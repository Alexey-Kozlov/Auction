using System.Reflection;
using System.Text.Json;
using AutoMapper;
using Common.Contracts.Auction;
using Common.Contracts.Image;
using Common.Contracts.Processing;
using ImageService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ImageService.Consumers;

public class ImageConsumer : IConsumer<DataForProcessingServicesList<ImageDTO>>
{
    private readonly ImageDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public ImageConsumer(ImageDbContext context, IPublishEndpoint publishEndpoint,
        IConfiguration configuration, IMapper mapper)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
        _mapper = mapper;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<ImageDTO>> context)
    {
        //для работы с аукционом всегда передается только 1 изображение (или не передается) 
        var correlationId = context.Message.CorrelationId;
        if (context.Message.DataObjects.Any())
        {
            var typedItem = JsonSerializer.Deserialize<ImageDTO>(context.Message.DataObjects[0].Data);

            switch (context.Message.DataObjects[0].CRUD)
            {
                case CRUD.Delete:
                    var item = await _context.Images.FirstOrDefaultAsync(p => p.AuctionId == typedItem.AuctionId);
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

                    var item2 = await _context.Images.FirstOrDefaultAsync(p => p.AuctionId == typedItem.AuctionId);
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

            await _context.SaveChangesAsync();
        }

        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
        await _publishEndpoint.Publish(sendObject);
    }
}
