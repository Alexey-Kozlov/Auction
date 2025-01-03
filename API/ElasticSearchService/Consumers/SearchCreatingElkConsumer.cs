using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.ELKSearch;
using Common.Contracts.Processing;
using Common.Utils;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using ElasticSearchService.Services;
using MassTransit;

namespace ElasticSearchService.Consumers;

public class SearchCreatingElkConsumer : IConsumer<DataForProcessingServicesList<ElkSearchCreating>>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ElkClient _client;

    public SearchCreatingElkConsumer(IPublishEndpoint publishEndpoint, ElkClient client)
    {
        _publishEndpoint = publishEndpoint;
        _client = client;
    }
    public async Task Consume(ConsumeContext<DataForProcessingServicesList<ElkSearchCreating>> consumeContext)
    {
        var typed = JsonSerializer.Deserialize<ElkSearchCreating>(consumeContext.Message.DataObjects[0].Data);
        //запрос на получение количества возвращаемых записей
        var countResponse = await _client.Client.CountAsync<AuctionCreatingElk>(s => s
            .Query(q => q
                .Bool(b => b
                    .Should(s => s
                       .Match(m => m
                           .Field(f => f.Title)
                            .Fuzziness(new Fuzziness("AUTO"))
                            .Query(typed.SearchTerm)
                            .Operator(Operator.And)
                        ),
                        s => s
                       .Match(m => m
                           .Field(f => f.Description)
                            .Fuzziness(new Fuzziness("AUTO"))
                            .Query(typed.SearchTerm)
                            .Operator(Operator.And)
                        ),
                        s => s
                       .Match(m => m
                           .Field(f => f.Properties)
                            .Fuzziness(new Fuzziness("AUTO"))
                            .Query(typed.SearchTerm)
                            .Operator(Operator.And)
                        )
                    )
                )
                
            )
        );
        var elkResponse = await _client.Client.SearchAsync<AuctionCreatingElk>(s => s
            .From((typed.PageNumber - 1) * typed.PageSize)
            .Size(typed.PageSize)
            .TrackTotalHits(new Elastic.Clients.Elasticsearch.Core.Search.TrackHits(true))
            .Sort()
            //запрос - поисковый запрос разбивается на термы, все термы должны быть
            //указанном поле. Поиск нечеткий (Fuzzy), с учетом русского языка.
            //поиск по ИЛИ в 3-х полях - Title, Properties, Description
            .Query(q => q
                .Bool(b => b
                    .Should(s => s
                       .Match(m => m
                           .Field(f => f.Title)
                            .Fuzziness(new Fuzziness("AUTO"))
                            .Query(typed.SearchTerm)
                            .Operator(Operator.And)
                        ),
                        s => s
                       .Match(m => m
                           .Field(f => f.Description)
                            .Fuzziness(new Fuzziness("AUTO"))
                            .Query(typed.SearchTerm)
                            .Operator(Operator.And)
                        ),
                        s => s
                       .Match(m => m
                           .Field(f => f.Properties)
                            .Fuzziness(new Fuzziness("AUTO"))
                            .Query(typed.SearchTerm)
                            .Operator(Operator.And)
                        )
                    )
                )
            )
        );

        var itemsCount = (int)countResponse.Count;
        var pageCount = 0;
        if (itemsCount > 0)
        {
            pageCount = (itemsCount + typed.PageSize - 1) / typed.PageSize;
        }

        var resultData = new ApiResponse<PagedResult<List<AuctionCreatingElk>>>
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = new PagedResult<List<AuctionCreatingElk>>()
            {
                Results = elkResponse.IsValidResponse ?  elkResponse.Documents.ToList() :  new List<AuctionCreatingElk>(),
                PageCount = pageCount,
                TotalCount = itemsCount
            }
        };

        var result = new ElkSearchCreated<ApiResponse<PagedResult<List<AuctionCreatingElk>>>>
        {
            CorrelationId = consumeContext.Message.CorrelationId,
            SearchTerm = typed.SearchTerm,
            ResultType = ResultType.Success,
            Result = resultData
        };

        await _publishEndpoint.Publish(result);
    }
}
