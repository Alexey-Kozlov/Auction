using ImageService.Data;
using AutoMapper;
using Contracts;
using ImageService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ImageService.Consumers;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
{
    private readonly IMapper _mapper;
    private readonly ImageDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionUpdatedConsumer(IMapper mapper, ImageDbContext context, IPublishEndpoint publishEndpoint)
    {
        _mapper = mapper;
        _context = context;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<AuctionUpdated> consumeContext)
    {
        if (string.IsNullOrEmpty(consumeContext.Message.Image)) return;
        Console.WriteLine($"{DateTime.Now} Получение сообщения обновить изображение для аукциона");
        var updatedItem = _mapper.Map<ImageItem>(consumeContext.Message);
        var item = await _context.Images.FirstOrDefaultAsync(p => p.AuctionId == updatedItem.Id);
        if (item != null)
        {
            _mapper.Map(updatedItem, item);
        }
        else
        {
            await _context.AddAsync(updatedItem);
        }
        await _context.SaveChangesAsync();
        //послать сообщение о сбросе этого мзображения в кеше редис
        await _publishEndpoint.Publish(new ImageUpdated(consumeContext.Message.Id));
    }
}
