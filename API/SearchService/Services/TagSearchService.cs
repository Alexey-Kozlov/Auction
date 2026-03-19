using Common.Contracts;
using Common.Contracts.Auction;
using Common.Contracts.Report;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;
using SearchService.DTO;

namespace SearchService.Services;

public class TagSearchService
{
    private readonly SearchDbContext _dbContext;
    private readonly GrpcTagClient _grpcTagClient;

    public TagSearchService(SearchDbContext dbContext, GrpcTagClient grpcTagClient)
    {
        _dbContext = dbContext;
        _grpcTagClient = grpcTagClient;
    }

    //поиск аукционов по тегу
    public async Task<ApiResponse<PagedResult<List<AuctionItem>>>> GetTagAuctionItems(SearchParamsDTO searchParams)
    {
        // делаем дополнительный запрос к микросервису тегов для получения списка ID аукционов,
        // содержащим указанный тег
        var query = new SqlQuery();
        query.Text = "select \"AuctionId\" from \"TagItems\" where \"Tag\"={0}";
        query.Parameters.Add(searchParams.Tag);
        query.Text += " limit 3000";

        //получаем список AuctionId по указанному тегу
        var _auctionIdList = await _grpcTagClient.GetTagAuctionItems(query);
        var auctionIdList = _auctionIdList.Result;

        //отбираем аукционы по полученным AuctionId
        var queryAuction = _dbContext.AuctionItems.AsQueryable();
        queryAuction = queryAuction.Where(p => auctionIdList.Contains(p.ItemId) && p.Commited);
        //сортировка в зависимости от текстового параметра OrderBy
        queryAuction = searchParams.OrderBy switch
        {
            "titleAsc" => queryAuction.OrderBy(p => p.Title).ThenBy(p => p.ItemId),
            "titleDesc" => queryAuction.OrderByDescending(p => p.Title).ThenByDescending(p => p.ItemId),
            "newAsc" => queryAuction.OrderBy(p => p.CreateAt).ThenBy(p => p.ItemId),
            "newDesc" => queryAuction.OrderByDescending(p => p.CreateAt).ThenByDescending(p => p.ItemId),
            "endAsc" => queryAuction.OrderBy(p => p.AuctionEnd).ThenBy(p => p.ItemId),
            _ => queryAuction.OrderByDescending(p => p.AuctionEnd).ThenByDescending(p => p.ItemId)
        };

        var itemsCount = await queryAuction.CountAsync();
        var pageCount = 0;
        if (itemsCount > 0)
        {
            pageCount = (itemsCount + searchParams.PageSize - 1) / searchParams.PageSize;
        }
        if (searchParams.PageNumber > pageCount)
        {
            searchParams.PageNumber = pageCount == 0 ? 1 : pageCount;
        }
        var result = await queryAuction.Skip((searchParams.PageNumber - 1) * searchParams.PageSize)
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
}
