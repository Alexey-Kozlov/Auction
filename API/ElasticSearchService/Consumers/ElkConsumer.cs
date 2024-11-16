using System.Reflection;
using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Processing;
using ElasticSearchService.Services;
using MassTransit;

namespace ElasticSearchService.Consumers;

public class ElkConsumer : IConsumer<DataForProcessingServicesList<AuctionItem>>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ElkClient _client;
    private readonly IConfiguration _configuration;

    public ElkConsumer(IPublishEndpoint publishEndpoint, ElkClient client, IConfiguration configuration)
    {
        _publishEndpoint = publishEndpoint;
        _client = client;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<AuctionItem>> context)
    {
        var typedItem = JsonSerializer.Deserialize<AuctionItem>(context.Message.DataObjects[0].Data);
        var correlationId = context.Message.CorrelationId;
        var response = await _client.Client.DeleteAsync(index: "search_index", typedItem.AuctionId.ToString());

        if (response.IsValidResponse)
        {
            Console.WriteLine($"Deleted document with ID {response.Id} succeeded.");
        }
        else
        {
            Console.WriteLine(response.ElasticsearchServerError);
        }
        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, correlationId);
        await _publishEndpoint.Publish(sendObject);
    }
}
