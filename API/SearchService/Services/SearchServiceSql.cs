using Common.Contracts;
using Common.Contracts.Auction;
using Common.Contracts.ELKSearch;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;
using SearchService.DTO;

namespace SearchService.Services;

public class SearchServiceSql
{
    private readonly SearchDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public SearchServiceSql(SearchDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    //SQL поиск по частичному точному совпадению в полях title, properties
    //также производим сортировку и фильтрацию по категориям
    public async Task<ApiResponse<PagedResult<List<AuctionItem>>>> SqlSearchItems(SearchParamsDTO searchParams)
    {
        var query = _context.AuctionItems.AsQueryable();
        query = query.Where(p => p.Commited);
        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            query = query.Where(p => p.Title.ToLower().Contains(searchParams.SearchTerm.ToLower()) ||
            p.Properties.ToLower().Contains(searchParams.SearchTerm.ToLower()));
        }
        //сортировка в зависимости от текстового параметра OrderBy
        query = searchParams.OrderBy switch
        {
            "titleAsc" => query.OrderBy(p => p.Title).ThenBy(p => p.ItemId),
            "titleDesc" => query.OrderByDescending(p => p.Title).ThenByDescending(p => p.ItemId),
            "newAsc" => query.OrderBy(p => p.CreateAt).ThenBy(p => p.ItemId),
            "newDesc" => query.OrderByDescending(p => p.CreateAt).ThenByDescending(p => p.ItemId),
            "endAsc" => query.OrderBy(p => p.AuctionEnd).ThenBy(p => p.ItemId),
            _ => query.OrderByDescending(p => p.AuctionEnd).ThenByDescending(p => p.ItemId)
        };
        //отбор в зависимости от текстового параметра FilterBy
        if (!string.IsNullOrEmpty(searchParams.FilterBy))
        {
            query = searchParams.FilterBy switch
            {
                "finished" => query.Where(p => p.Finished),
                "endingSoon" => query.Where(p => p.AuctionEnd < DateTime.UtcNow.AddHours(24) && !p.Finished),
                _ => query.Where(p => !p.Finished)
            };
        }

        //если ищем свои аукционы
        if (!string.IsNullOrEmpty(searchParams.Seller))
        {
            query = query.Where(p => p.Seller == searchParams.Seller);
        }

        //если ищем выигранные аукционы
        if (!string.IsNullOrEmpty(searchParams.Winner))
        {
            query = query.Where(p => p.Winner == searchParams.Winner);
        }
        var itemsCount = await query.CountAsync();
        var pageCount = 0;
        if (itemsCount > 0)
        {
            pageCount = (itemsCount + searchParams.PageSize - 1) / searchParams.PageSize;
        }
        if (searchParams.PageNumber > pageCount)
        {
            searchParams.PageNumber = pageCount == 0 ? 1 : pageCount;
        }
        var result = await query.Skip((searchParams.PageNumber - 1) * searchParams.PageSize)
        .Take(searchParams.PageSize)
        .ToListAsync();


        return new ApiResponse<PagedResult<List<AuctionItem>>>
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = new PagedResult<List<AuctionItem>>()
            {
                Results = result,
                PageCount = pageCount,
                TotalCount = itemsCount
            }
        };
    }

    public async Task<ApiResponse<PagedResult<List<AuctionItem>>>> ElkSearchItems(SearchParamsDTO searchParams)
    {
        //посылаем сообщение для поиска в ELK
        await _publishEndpoint.Publish(new ElkSearchRequest(Guid.NewGuid(), Guid.NewGuid(),
            searchParams.SearchAdv, searchParams.PageNumber, searchParams.PageSize, searchParams.SessionId));

        //посылаем null в качестве результата для отображения заставки ожидания
        return new ApiResponse<PagedResult<List<AuctionItem>>>
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = null
        };
    }

    public async Task<ApiResponse<AuctionItem>> SearchItemById(string id)
    {
        var item = await _context.AuctionItems.Where(p => p.ItemId == Guid.Parse(id) && p.Commited).FirstOrDefaultAsync();
        return new ApiResponse<AuctionItem>
        {
            IsSuccess = true,
            StatusCode = System.Net.HttpStatusCode.OK,
            Result = item
        };
    }
}
