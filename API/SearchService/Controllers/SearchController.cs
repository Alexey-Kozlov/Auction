using Common.Contracts;
using Common.Contracts.Auction;
using Common.Contracts.Report;
using Microsoft.AspNetCore.Mvc;
using SearchService.DTO;

namespace SearchService.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly Services.SearchServiceSql _search;

    public SearchController(Services.SearchServiceSql search)
    {
        _search = search;
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<AuctionItem>> SearchItemById(string id)
    {
        //поиск по id
        return await _search.SearchItemById(id);
    }

    [HttpGet]
    public async Task<ApiResponse<PagedResult<List<AuctionItem>>>> SearchItems([FromQuery] SearchParamsDTO searchParams)
    {
        //если заполнен параметр SearchAdv - это означает, что поступил запрос на поиск
        //в ElasticSearch. В этом случае направляем запрос через шину сообщений в сервис ElasticSearchService
        if (!string.IsNullOrEmpty(searchParams.SearchAdv))
        {
            return await _search.ElkSearchItems(searchParams);
        }

        //а здесь обычный SQL-поиск с точным частичным вхождением поисковой последовательности в поля title, properties
        return await _search.SqlSearchItems(searchParams);
    }

    [HttpPost("GetAuctionItemsByQuery")]
    public async Task<string> GetAuctionItemsByQuery(ReportParamsDTO dto)
    {
        return System.Text.Json.JsonSerializer.Serialize(await _search.GetAuctionItemsByQuery(dto.Expression));
    }

}