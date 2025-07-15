using System.Text.Json;
using Common.Contracts;
using Common.Contracts.Finance;
using Common.Utils.Extentions;
using FinanceService;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;

namespace SearchService.Services;

public class GrpcFinanceService: GrpcFinance.GrpcFinanceBase
{
    private readonly SearchDbContext _dbContext;
    public GrpcFinanceService(SearchDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // получили от FinanceService список финансовых записей, здесь добавляем к этим записям 
    // данные по автору аукциона (поле Seller) и наименованию аукциона (поле Title)
    public override async Task<GrpcFinanceResponse> GetFinance(GetFinanceRequest request, ServerCallContext context)
    {
        var financeSortRequest = JsonSerializer.Deserialize<FinanceSortRequest>(request.FinanceSortRequest);
        var ids = financeSortRequest.FinanceItems.Where(p => p.AuctionId.HasValue)
            .Select(p => p.AuctionId).ToArray();
        var auctionList = await _dbContext.AuctionItems.Where(p => ids.Contains(p.ItemId))
        .Select(p => new { p.ItemId, p.Title, p.Seller }).ToListAsync();
        //объединяем результаты - исходные записи финансов и дополняем записями аукционов (поля Seller и Title)
        var rezult = financeSortRequest.FinanceItems.LeftOuterJoin(
            auctionList,
            leftKey => leftKey.AuctionId,
            rightKey => rightKey.ItemId,
            (fin, auction) => new FinanceHistoryItem
            {
                ActionDate = fin.ActionDate,
                AuctionId = fin.AuctionId,
                AuctionSeller = auction?.Seller,
                AuctionTitle = auction?.Title,
                ItemId = fin.ItemId,
                Status = fin.Status,
                UserLogin = fin.UserLogin,
                Value = fin.Value
            }
        );
        //сортируем по заданному полю
        rezult = financeSortRequest.OrderBy switch
        {
            "titleAsc" => rezult.OrderBy(p => p.AuctionTitle).ThenBy(p => p.ActionDate),
            "titleDesc" => rezult.OrderByDescending(p => p.AuctionTitle).ThenBy(p => p.ActionDate),
            "actionDateAsc" => rezult.OrderBy(p => p.ActionDate).ThenBy(p => p.AuctionId),
            "actionDateDesc" => rezult.OrderByDescending(p => p.ActionDate).ThenBy(p => p.AuctionId),
            "sellerAsc" => rezult.OrderBy(p => p.AuctionSeller).ThenBy(p => p.ActionDate),
            "sellerDesc" => rezult.OrderByDescending(p => p.AuctionSeller).ThenBy(p => p.ActionDate),
            "statusAsc" => rezult.OrderBy(p => p.Status).ThenBy(p => p.ActionDate),
            "statusDesc" => rezult.OrderByDescending(p => p.Status).ThenBy(p => p.ActionDate),
            "valueAsc" => rezult.OrderBy(p => p.Value).ThenBy(p => p.ActionDate),
            "valueDesc" => rezult.OrderByDescending(p => p.Value).ThenBy(p => p.ActionDate),
            _ => rezult.OrderBy(p => p.ActionDate)
        };
        var pageCount = (rezult.Count() + financeSortRequest.PageSize - 1) / financeSortRequest.PageSize;
        var totalCount = rezult.Count();
        //добавляем пагинацию
        rezult = rezult.Skip((financeSortRequest.PageNumber - 1) * financeSortRequest.PageSize)
            .Take(financeSortRequest.PageSize);        
        var message = new ApiResponse<PagedResult<List<FinanceHistoryItem>>>
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = new PagedResult<List<FinanceHistoryItem>>()
            {
                Results = rezult.ToList(),
                PageCount = pageCount,
                TotalCount = totalCount
            }
        };
        //возвращаем результат
        var response = new GrpcFinanceResponse
        {
            FinancePagedRezult = new FinanceHistoryModel
            {
                FinancePagedRezult = JsonSerializer.Serialize(message, message.GetType())
            }
        };
        return response;
    }
}
