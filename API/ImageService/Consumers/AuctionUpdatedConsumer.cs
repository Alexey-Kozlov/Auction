using AuctionService.Data;
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

    public AuctionUpdatedConsumer(IMapper mapper, ImageDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }
    public async Task Consume(ConsumeContext<AuctionUpdated> consumeContext)
    {
        Console.WriteLine("--> Получение сообщения обновить аукцион");
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
    }
}
