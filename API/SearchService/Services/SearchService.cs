using Common.Contracts.Auction;
using Common.Contracts.ELKSearch;
using Common.Utils;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;
using SearchService.DTO;

namespace SearchService.Services;

public class SearchService
{
    private readonly SearchDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public SearchService(SearchDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    //SQL поиск по частичному точному совпадению в полях title, properties
    //также производим сортировку и фильтрацию по категориям
    public async Task<ApiResponse<PagedResult<List<AuctionItem>>>> SqlSearchItems(SearchParamsDTO searchParams)
    {
        var query = _context.AuctionItems.AsQueryable();
        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            query = query.Where(p => p.Title.ToLower().Contains(searchParams.SearchTerm.ToLower()) ||
            p.Properties.ToLower().Contains(searchParams.SearchTerm.ToLower()));
        }
        //сортировка в зависимости от текстового параметра OrderBy
        query = searchParams.OrderBy switch
        {
            "titleAsc" => query.OrderBy(p => p.Title).ThenBy(p => p.Title),
            "titleDesc" => query.OrderByDescending(p => p.Title).ThenByDescending(p => p.Title),
            "newAsc" => query.OrderBy(p => p.CreateAt).ThenBy(p => p.Title),
            "newDesc" => query.OrderByDescending(p => p.CreateAt).ThenByDescending(p => p.Title),
            "endAsc" => query.OrderBy(p => p.AuctionEnd),
            _ => query.OrderByDescending(p => p.AuctionEnd)
        };
        //отбор в зависимости от текстового параметра FilterBy
        if (!string.IsNullOrEmpty(searchParams.FilterBy))
        {
            query = searchParams.FilterBy switch
            {
                "finished" => query.Where(p => p.AuctionEnd < DateTime.UtcNow),
                "endingSoon" => query.Where(p => p.AuctionEnd < DateTime.UtcNow.AddHours(24)
                    && p.AuctionEnd > DateTime.UtcNow),
                _ => query.Where(p => p.AuctionEnd > DateTime.UtcNow)
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
        var result = await query.Skip((searchParams.PageNumber - 1) * searchParams.PageSize)
            .Take(searchParams.PageSize)
            .ToListAsync();
        var pageCount = 0;
        if (itemsCount > 0)
        {
            pageCount = (itemsCount + searchParams.PageSize - 1) / searchParams.PageSize;
        }

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

        Console.WriteLine($"Поиск ELK - '{searchParams.SearchAdv}' сессия - {searchParams.SessionId}");

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
        var item = await _context.AuctionItems.Where(p => p.AuctionId == Guid.Parse(id)).FirstOrDefaultAsync();
        return new ApiResponse<AuctionItem>
        {
            IsSuccess = true,
            StatusCode = System.Net.HttpStatusCode.OK,
            Result = item
        };
    }
}