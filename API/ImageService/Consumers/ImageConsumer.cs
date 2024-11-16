using System.Reflection;
using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Processing;
using ImageService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ImageService.Entities;

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
        if (context.Message.DataObjects[0].CRUD == CRUD.Delete)
        {
            var item = await _context.Images.FirstOrDefaultAsync(p => p.AuctionId == typedItem.AuctionId);
            if (item != null)
            {
                _context.Images.Remove(item);
            }
        }
        if (context.Message.DataObjects[0].CRUD == CRUD.Create)
        {
            await _context.Images.AddAsync(new ImageItem
            {
                AuctionId = typedItem.AuctionId,
                Image = Convert.FromBase64String(typedItem.Winner
                         .Replace("data:image/png;base64,", "")
                         .Replace("data:image/jpeg;base64,", "")
                         .Replace("data:image/jpg;base64,", ""))
            });
        }
        await _context.SaveChangesAsync();
        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
        await _publishEndpoint.Publish(sendObject);
    }
}
