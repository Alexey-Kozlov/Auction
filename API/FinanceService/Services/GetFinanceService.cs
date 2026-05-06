using System.Text.Json;
using Common.Contracts;
using Common.Contracts.Finance;
using FinanceService.Data;
using FinanceService.DTO;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Services;

public class GetFinanceService
{
    private readonly FinanceDbContext _context;
    private readonly GrpcFinanceClient _grpcFinanceClient;

    public GetFinanceService(FinanceDbContext context, GrpcFinanceClient grpcFinanceClient)
    {
        _context = context;
        _grpcFinanceClient = grpcFinanceClient;
    }

    public async Task<ApiResponse<int>> GetBalance(string userLogin)
    {
        var balanceItem = await _context.FinanceItems.Where(p => p.Commited && p.UserLogin == userLogin &&
            p.Status == FinanceRecordStatus.Баланс).FirstOrDefaultAsync();

        return new ApiResponse<int>()
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = balanceItem?.Value ?? 0
        };
    }

    public async Task<ApiResponse<PagedResult<List<FinanceHistoryItem>>>> GetHistory(
        PagedParamsDTO pagedParamsDTO, string userLogin)
    {
        var historyItemList = await _context.FinanceItems.Where(p => p.Commited &&
            p.UserLogin == userLogin &&
            p.Status != FinanceRecordStatus.Баланс).Select(p => new FinanceHistoryItem
            {
                ActionDate = p.ActionDate,
                AuctionId = p.AuctionId,
                ItemId = p.ItemId,
                Status = p.Status,
                UserLogin = p.UserLogin,
                Value = p.Value
            }).ToListAsync();

        //посылаем на обработку в сервис SearchService - для добавления значений полей auctionTitle и auctionSeller
        //и сортировки по нужному полю, отсылаем асинхронно через Rabbit
        var sortRequest = new FinanceSortRequest
        {
            FinanceItems = historyItemList,
            OrderBy = pagedParamsDTO.OrderBy,
            PageNumber = pagedParamsDTO.PageNumber,
            PageSize = pagedParamsDTO.PageSize
        };
        var jsonRezult = _grpcFinanceClient.GetFinance(JsonSerializer.Serialize(sortRequest, sortRequest.GetType()))
            .GetAwaiter().GetResult();
        return JsonSerializer.Deserialize<ApiResponse<PagedResult<List<FinanceHistoryItem>>>>(jsonRezult);
    }
}