using ImageService.Data;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ImageService.Consumers;

public class AuctionDeletingImageConsumer : IConsumer<AuctionDeletingImage>
{
    private readonly ImageDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionDeletingImageConsumer(ImageDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<AuctionDeletingImage> consumeContext)
    {
        Console.WriteLine($"{DateTime.Now}  Получение сообщения удалить аукцион");
        var item = await _context.Images.FirstOrDefaultAsync(p => p.AuctionId == consumeContext.Message.Id);
        if (item != null)
        {
            _context.Images.Remove(item);
            await _context.SaveChangesAsync();
        }
        await _publishEndpoint.Publish(new AuctionDeletedImage(consumeContext.Message.CorrelationId));
    }
}
