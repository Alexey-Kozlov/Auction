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

        var sqlQuery = _dbContext.AuctionItems.Where(p => auctionIdList.Contains(p.ItemId) && p.Commited)
            .OrderBy(p => p.Title).ThenBy(p => p.ItemId);

        var itemsCount = await sqlQuery.CountAsync();
        var pageCount = 0;
        if (itemsCount > 0)
        {
            pageCount = (itemsCount + searchParams.PageSize - 1) / searchParams.PageSize;
        }
        if (searchParams.PageNumber > pageCount)
        {
            searchParams.PageNumber = pageCount == 0 ? 1 : pageCount;
        }
        var result = await sqlQuery.Skip((searchParams.PageNumber - 1) * searchParams.PageSize)
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
