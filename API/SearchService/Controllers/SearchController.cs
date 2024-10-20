using Common.Utils;
using Microsoft.AspNetCore.Mvc;
using SearchService.DTO;
using SearchService.Entities;

namespace SearchService.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly Services.SearchService _search;

    public SearchController(Services.SearchService search)
    {
        _search = search;
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<Item>> SearchItemById(string id)
    {
        //поиск по id
        return await _search.SearchItemById(id);
    }

    [HttpGet]
    public async Task<ApiResponse<PagedResult<List<Item>>>> SearchItems([FromQuery] SearchParamsDTO searchParams)
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
}