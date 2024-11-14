using System.Reflection;
using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Processing;
using ImageService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ImageService.Consumers;

public class ImageConsumer : IConsumer<DataForProcessingServicesList<AuctionItem>>
{
    private readonly ImageDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public ImageConsumer(ImageDbContext context, IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<AuctionItem>> context)
    {
        var typedItem = JsonSerializer.Deserialize<AuctionItem>(context.Message.DataObjects[0].Data);
        var correlationId = context.Message.CorrelationId;
        var item = await _context.Images.FirstOrDefaultAsync(p => p.AuctionId == typedItem.AuctionId);
        if (item != null)
        {
            _context.Images.Remove(item);
            await _context.SaveChangesAsync();
        }
        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
        await _publishEndpoint.Publish(sendObject);
    }
}
