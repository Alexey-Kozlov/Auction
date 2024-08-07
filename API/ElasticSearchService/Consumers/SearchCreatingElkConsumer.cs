using Common.Contracts;
using Common.Utils;
using ElasticSearchService.Services;
using MassTransit;

namespace ElasticSearchService.Consumers;

public class SearchCreatingElkConsumer : IConsumer<ElkSearchCreating>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ElkClient _client;

    public SearchCreatingElkConsumer(IPublishEndpoint publishEndpoint, ElkClient client)
    {
        _publishEndpoint = publishEndpoint;
        _client = client;
    }
    public async Task Consume(ConsumeContext<ElkSearchCreating> consumeContext)
    {

        var elkResponse = await _client.Client.SearchAsync<AuctionCreatingElk>(s =>
            s.From(consumeContext.Message.PageNumber - 1)
            .Size(consumeContext.Message.PageSize)
            .Query(q =>
                q.QueryString(p =>
                p.Query(consumeContext.Message.SearchTerm)
                ))
        );

        var itemsCount = elkResponse.Documents.Count;
        var pageCount = 0;
        if (itemsCount > 0)
        {
            pageCount = (itemsCount + consumeContext.Message.PageSize - 1) / consumeContext.Message.PageSize;
        }

        if (elkResponse.IsValidResponse)
        {
            Console.WriteLine($"{DateTime.Now} - По запросу {consumeContext.Message.SearchTerm} найдено {elkResponse.Documents.Count} записей.");
        }
        else
        {
            throw new Exception("Ошибка ELK-сервиса");
        }

        var resultData = new ApiResponse<PagedResult<List<AuctionCreatingElk>>>
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = new PagedResult<List<AuctionCreatingElk>>()
            {
                Results = elkResponse.Documents.ToList(),
                PageCount = pageCount,
                TotalCount = itemsCount
            }
        };

        var result = new ElkSearchCreated<ApiResponse<PagedResult<List<AuctionCreatingElk>>>>(
            consumeContext.Message.CorrelationId,
            consumeContext.Message.SearchTerm,
            ResultType.Success,
            resultData
        );

        await _publishEndpoint.Publish(result);
    }
}
