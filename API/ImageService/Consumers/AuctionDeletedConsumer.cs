using AuctionService.Data;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ImageService.Consumers;

public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
{
    private readonly ImageDbContext _context;

    public AuctionDeletedConsumer(ImageDbContext context)
    {
        _context = context;
    }
    public async Task Consume(ConsumeContext<AuctionDeleted> consumeContext)
    {
        Console.WriteLine("--> Получение сообщения удалить аукцион");
        var item = await _context.Images.FirstOrDefaultAsync(p => p.AuctionId == consumeContext.Message.Id);
        if (item != null)
        {
            _context.Images.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}
