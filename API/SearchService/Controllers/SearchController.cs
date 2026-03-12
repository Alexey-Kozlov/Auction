using Common.Contracts;
using Common.Contracts.Auction;
using Microsoft.AspNetCore.Mvc;
using SearchService.DTO;
using SearchService.Services;

namespace SearchService.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly SearchServiceSql _search;
    private readonly TagSearchService _tag;

    public SearchController(SearchServiceSql search, TagSearchService tag)
    {
        _search = search;
        _tag = tag;
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
        //если заполнен параметр Tag - реализуем поиск по тегам, это потребует выделенный сервис
        if (!string.IsNullOrEmpty(searchParams.Tag))
        {
            return await _tag.GetTagAuctionItems(searchParams);
        }
        //если заполнен параметр SearchAdv - это означает, что поступил запрос на поиск
        //в ElasticSearch. В этом случае направляем запрос через шину сообщений в сервис ElasticSearchService
        //и ничего не возвращаем
        if (!string.IsNullOrEmpty(searchParams.SearchAdv))
        {
            return await _search.ElkSearchItems(searchParams);
        }
        //а здесь обычный SQL-поиск с точным вхождением поисковой последовательности в поля title, description, properties
        //возвращаем полученные записи в ответ, синхронное взаимодействие
        return await _search.SqlSearchItems(searchParams);
    }

}