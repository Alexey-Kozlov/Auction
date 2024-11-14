using System.Reflection;
using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Processing;
using GatewayService.Cache;
using MassTransit;

namespace GatewayService.Consumers;

public class GatewayConsumer : IConsumer<DataForProcessingServicesList<AuctionItem>>
{
    private readonly ImageCache _cacheService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;
    public GatewayConsumer(ImageCache cacheService, IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _cacheService = cacheService;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<AuctionItem>> context)
    {
        var typedItem = JsonSerializer.Deserialize<AuctionItem>(context.Message.DataObjects[0].Data);
        var correlationId = context.Message.CorrelationId;
        await _cacheService.DeleteCacheItem(typedItem.AuctionId.ToString());
        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
        await _publishEndpoint.Publish(sendObject);
    }
}
