using Common.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;
using SearchService.Entities;

namespace SearchService.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly SearchDbContext _context;

    public SearchController(SearchDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    public async Task<ApiResponse<SearchType<List<Item>>>> SearchItems([FromQuery] SearchParams searchParams)
    {
        var query = _context.Items.AsQueryable();
        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            query = query.Where(p => p.Title.Contains(searchParams.SearchTerm, StringComparison.CurrentCultureIgnoreCase) ||
            p.Properties.Contains(searchParams.SearchTerm, StringComparison.CurrentCultureIgnoreCase));
        }
        //сортировка в зависимости от текстового параметра OrderBy
        query = searchParams.OrderBy switch
        {
            "title" => query.OrderBy(p => p.Title).ThenBy(p => p.Title),
            "new" => query.OrderByDescending(p => p.CreateAt).ThenBy(p => p.Title),
            _ => query.OrderBy(p => p.AuctionEnd)
        };

        //отбор в зависимости от текстового параметра FilterBy
        query = searchParams.FilterBy switch
        {
            "finished" => query.Where(p => p.AuctionEnd < DateTime.UtcNow),
            "endingSoon" => query.Where(p => p.AuctionEnd < DateTime.UtcNow.AddHours(24)
                && p.AuctionEnd > DateTime.UtcNow),
            _ => query.Where(p => p.AuctionEnd > DateTime.UtcNow)
        };

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
        var result = await query.Skip((searchParams.PageNumber - 1) * searchParams.PageSize)
            .Take(searchParams.PageSize)
            .ToListAsync();
        var pageCount = 0;
        var pageNumber = searchParams.PageNumber;
        if (result.Count > 0)
        {
            pageCount = result.Count / searchParams.PageSize;
        }

        return new ApiResponse<SearchType<List<Item>>>
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = new SearchType<List<Item>>()
            {
                Results = result,
                PageCount = pageCount,
                TotalCount = result.Count
            }
        };
    }

    public class SearchType<T>
    {
        public T Results { get; set; }
        public int PageCount { get; set; }
        public int TotalCount { get; set; }
    }
}