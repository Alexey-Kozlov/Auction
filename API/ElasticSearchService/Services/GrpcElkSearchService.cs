using System.Text.Json;
using Common.Contracts.ELKSearch;
using ElasticSearchService.Services.Search;
using ElkSearchService;
using Grpc.Core;


namespace ElasticSearchService.Services;

public class GrpcElkSearchService : GrpcElk.GrpcElkBase
{
    private readonly SearchElk _searchElk;

    public GrpcElkSearchService(SearchElk searchElk)
    {
        _searchElk = searchElk;
    }

    // получили запрос для выборки данных по аукционам
    public override Task<GetElkSearchResponse> GetElkSearch(GetElkSearchRequest request, ServerCallContext context)
    {
        var requestItems = JsonSerializer.Deserialize<ElkSearchRequest>(request.ElkSearchRequest);
        return Task.FromResult(new GetElkSearchResponse
        {
            ElkSearchResult = JsonSerializer.Serialize(_searchElk.SearchItems(requestItems).Result)
        });
    }
}
