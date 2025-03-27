using System.Reflection;
using Common.Contracts.ELKSearch;
using Common.Contracts.Processing;
using ElasticSearchService.Services;
using MassTransit;

namespace ElasticSearchService.Consumers;

public class CommitElkConsumer : IConsumer<ElkCommit>
{
    private readonly ElkClient _client;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public CommitElkConsumer(ElkClient client, IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _client = client;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<ElkCommit> context)
    {

        var correlationId = context.Message.CorrelationId;
        try
        {



        }
        catch (Exception e)
        {
            var errorItem = new NotificationServiceError
            {
                CorrelationId = context.Message.CorrelationId,
                Message = e.Message,
                ExceptionMessage = e.Source + "," + e.StackTrace,
                ServiceName = "ElkService",
                UserLogin = "",
                IsError = true,
                AuctionId = null,
                TraceId = Guid.NewGuid()
            };
            await _publishEndpoint.Publish(errorItem);
        }
    }
}
