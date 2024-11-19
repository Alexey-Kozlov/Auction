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
        switch (context.Message.DataObjects[0].CRUD)
        {
            case CRUD.Delete:
                var item = await _context.Images.FirstOrDefaultAsync(p => p.AuctionId == typedItem.AuctionId);
                if (item != null)
                {
                    _context.Images.Remove(item);
                    await _context.SaveChangesAsync();
                }
                break;
            case CRUD.Create:
                await _context.Images.AddAsync(new ImageItem
                {
                    AuctionId = typedItem.AuctionId,
                    Image = Convert.FromBase64String(context.Message.Props
                        .Replace("data:image/png;base64,", "")
                        .Replace("data:image/jpeg;base64,", "")
                        .Replace("data:image/jpg;base64,", ""))
                });
                await _context.SaveChangesAsync();
                break;
            case CRUD.Update:
                if (!string.IsNullOrEmpty(context.Message.Props))
                {
                    var image = Convert.FromBase64String(context.Message.Props
                            .Replace("data:image/png;base64,", "")
                            .Replace("data:image/jpeg;base64,", "")
                            .Replace("data:image/jpg;base64,", ""));
                    var item2 = await _context.Images.FirstOrDefaultAsync(p => p.AuctionId == typedItem.AuctionId);
                    if (item2 != null)
                    {
                        item2.Image = image;
                        _context.Images.Update(item2);
                    }
                    else
                    {
                        await _context.Images.AddAsync(new ImageItem
                        {
                            AuctionId = typedItem.AuctionId,
                            Image = image
                        });
                    }
                    await _context.SaveChangesAsync();
                }
                break;
        }


        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
        await _publishEndpoint.Publish(sendObject);
    }
}
