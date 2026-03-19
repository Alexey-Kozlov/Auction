using AutoMapper;
using Common.Contracts;
using Common.Contracts.Auction;
using Common.Contracts.ELKSearch;
using Elastic.Clients.Elasticsearch;

namespace ElasticSearchService.Services.Search;

public class SearchElk
{
    private readonly ElkClient _client;
    private readonly GetSearchItems _searchItems;
    private readonly IMapper _mapper;

    public SearchElk(ElkClient client, GetSearchItems searchItems, IMapper mapper)
    {
        _client = client;
        _searchItems = searchItems;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PagedResult<List<AuctionItem>>>> SearchItems(ElkSearchRequest context)
    {

        //получаем идентификаторы аукционов, где встречается искомая фраза - в аукционах, чатах и тегах
        var auctionIds = new List<string>();
        if (context.AdvSearchParam == null || context.AdvSearchParam.Length == 0) return new ApiResponse<PagedResult<List<AuctionItem>>>();

        //если был отмечен параметр поиска - аукционы (Auction)
        if (context.AdvSearchParam.Any(p => p == "Auction"))
        {
            auctionIds = await _searchItems.GetAuctionIds(context);
        }

        //если был отмечен параметр поиска - обсуждения (Comment)
        if (context.AdvSearchParam.Any(p => p == "Comment"))
        {
            auctionIds = await _searchItems.GetChatIds(context, auctionIds);
        }

        //если был отмечен параметр поиска - теги (Tag)
        if (context.AdvSearchParam.Any(p => p == "Tag"))
        {
            auctionIds = await _searchItems.GetTagIds(context, auctionIds);
        }

        //итоговый запрос на получение записей - ищем по полученному списку AuctionId
        var elkCount = await _client.Client.CountAsync<AuctionCreatingElk>(s => s
            .Query(q => q.TermsSet(p => p.Field(r => r.ItemId.Suffix("keyword")).Terms(auctionIds)
                .MinimumShouldMatch(1))));

        //получаем наименование поля для сортировки и направление сортировки
        var sortField = context.OrderBy.Replace("Asc", "").Replace("Desc", "");
        var sortDirection = context.OrderBy.IndexOf("Asc") == -1;
        //делаем актуальные поля сортировки
        switch (sortField)
        {
            case "title":
                sortField = "title.keyword";
                break;
            case "new":
                sortField = "auctionCreated";
                break;
            case "end":
                sortField = "auctionEnd";
                break;
            default:
                break;
        }

        var elkResponse = await _client.Client.SearchAsync<AuctionCreatingElk>(s => s
             .From((context.PageNumber - 1) * context.PageSize)
             .Size(context.PageSize)
             .TrackTotalHits(new Elastic.Clients.Elasticsearch.Core.Search.TrackHits(true))
             .Query(q => q.TermsSet(p => p.Field(r => r.ItemId.Suffix("keyword")).Terms(auctionIds)
                 .MinimumShouldMatch(1))
           //сортируем сначала по наименованию аукциона, потом по id (если одинаковые наименования)
           //Suffix - смотрим определение индекса - mappings в формате json, значение Suffix - keyword -
           //наименование свойства после "fields"
           ).Sort(p => p.Field(sortField, fs => fs.Order(sortDirection ? SortOrder.Desc : SortOrder.Asc)),
                p => p.Field(f => f.ItemId.Suffix("keyword"), fs => fs.Order(SortOrder.Asc)))
         );

        var itemsCount = Convert.ToInt32(elkCount.Count);
        var pageCount = 0;
        if (itemsCount > 0)
        {
            pageCount = (itemsCount + context.PageSize - 1) / context.PageSize;
        }

        return new ApiResponse<PagedResult<List<AuctionItem>>>
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = new PagedResult<List<AuctionItem>>()
            {
                Results = elkResponse.IsValidResponse ?
                    _mapper.Map<List<AuctionItem>>(elkResponse.Documents.ToList())
                    : new List<AuctionItem>(),
                PageCount = pageCount,
                TotalCount = itemsCount,
                PageNumber = context.PageNumber
            }
        };
    }
}
