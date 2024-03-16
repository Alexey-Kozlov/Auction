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
    public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
    {
        var query = _context.Items.AsQueryable();
        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            query = query.Where(p => p.Make.ToLower().Contains(searchParams.SearchTerm.ToLower()) ||
            p.Model.ToLower().Contains(searchParams.SearchTerm.ToLower()));
        }
        //сортировка в зависимости от текстового параметра OrderBy
        query = searchParams.OrderBy switch
        {
            "make" => query.OrderBy(p => p.Make).ThenBy(p => p.Model),
            "new" => query.OrderByDescending(p => p.CreateAt).ThenBy(p => p.Model),
            _ => query.OrderBy(p => p.AuctionEnd)
        };


        //отбор в зависимости от текстового параметра FilterBy
        query = searchParams.FilterBy switch
        {
            "finished" => query.Where(p => p.AuctionEnd < DateTime.UtcNow),
            "endingSoon" => query.Where(p => p.AuctionEnd < DateTime.UtcNow.AddHours(6)
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

        return Ok(
            new
            {
                results = result,
                pageCount = pageCount,
                totalCount = result.Count
            }
        );
    }
}