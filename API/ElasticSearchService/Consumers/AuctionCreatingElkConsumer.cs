﻿using Common.Contracts;
using ElasticSearchService.Services;
using MassTransit;

namespace ElasticSearchService.Consumers;

public class AuctionCreatingElkConsumer : IConsumer<AuctionCreatingElk>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ElkClient _client;

    public AuctionCreatingElkConsumer(IPublishEndpoint publishEndpoint, ElkClient client)
    {
        _publishEndpoint = publishEndpoint;
        _client = client;
    }
    public async Task Consume(ConsumeContext<AuctionCreatingElk> consumeContext)
    {
        var response = await _client.Client.IndexAsync(consumeContext.Message);

        if (response.IsValidResponse)
        {
            Console.WriteLine($"Index document with ID {response.Id} succeeded.");
        }
        else
        {
            Console.WriteLine(response.ElasticsearchServerError);
        }
        await _publishEndpoint.Publish(new AuctionCreatedElk(consumeContext.Message.CorrelationId));
        Console.WriteLine($"{DateTime.Now} --> Получение сообщения создать аукцион");
    }
}
