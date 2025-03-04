using System.Reflection;
using System.Text.Json;
using Common.Contracts;
using Common.Contracts.Auction;
using Common.Contracts.Processing;
using Common.Utils;
using Elastic.Clients.Elasticsearch;
using Logging.Services;
using MassTransit;

namespace Logging.Consumers;

public class LoggingConsumer : IConsumer<ItemLoggingContract>
{
    private readonly ElkClient _client;

    public LoggingConsumer(ElkClient client)
    {
        _client = client;
    }
    public async Task Consume(ConsumeContext<ItemLoggingContract> context)
    {
        await _client.Client.IndexAsync(context.Message, p => p.Index("logging_index"));
    }
}
