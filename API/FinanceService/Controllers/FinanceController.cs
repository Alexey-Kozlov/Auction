using System.Security.Claims;
using Common.Contracts;
using Common.Contracts.Finance;
using FinanceService.Data;
using FinanceService.DTO;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ImageService.Controllers;


[Authorize]
[ApiController]
[Route("api/finance")]
public class FinanceController : ControllerBase
{
    private readonly FinanceDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public FinanceController(FinanceDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet("GetBalance")]
    public async Task<ApiResponse<int>> GetBalance()
    {
        var userLogin = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login")
            .Select(p => p.Value).FirstOrDefault();
        var balanceItem = await _context.FinanceItems.Where(p => p.Commited && p.UserLogin == userLogin &&
            p.Status == FinanceRecordStatus.Баланс).FirstOrDefaultAsync();

        return new ApiResponse<int>()
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = balanceItem?.Value ?? 0
        };
    }

    [HttpGet("GetHistory")]
    public async Task<ApiResponse<PagedResult<List<BalanceItemDTO>>>> GetHistory([FromQuery] PagedParamsDTO pagedParams)
    {
        var userLogin = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login")
            .Select(p => p.Value).FirstOrDefault();
        var historyItemList = _context.FinanceItems.Where(p => p.Commited && p.UserLogin == userLogin &&
            p.Status != FinanceRecordStatus.Баланс)
            .OrderByDescending(p => p.ActionDate) as IQueryable<FinanceItem>;
        var pageCount = 0;
        var itemsCount = await historyItemList.CountAsync();
        var result = await historyItemList.Skip((pagedParams.PageNumber - 1) * pagedParams.PageSize)
            .Take(pagedParams.PageSize)
            .ToListAsync();
        if (itemsCount > 0)
        {
            pageCount = (itemsCount + pagedParams.PageSize - 1) / pagedParams.PageSize;
        }
        var resultDTO = new List<BalanceItemDTO>();
        foreach (var item in result)
        {
            resultDTO.Add(new BalanceItemDTO
            {
                AuctionId = item.AuctionId.HasValue ? item.AuctionId.Value : null,
                ItemId = item.ItemId.Value,
                UserLogin = item.UserLogin,
                Status = item.Status,
                ActionDate = item.ActionDate,
                Value = item.Value
            });
        }

        return new ApiResponse<PagedResult<List<BalanceItemDTO>>>
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = new PagedResult<List<BalanceItemDTO>>()
            {
                Results = resultDTO,
                PageCount = pageCount,
                TotalCount = itemsCount
            }
        };
    }

    [HttpGet("SortByColumn")]
    public async Task SortByColumn([FromQuery] PagedParamsDTO sortParams)
    {
        // запрос на сортировку по столбцу, отбираем все данные, без пежинации. 
        var userLogin = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login")
            .Select(p => p.Value).FirstOrDefault();
        var historyItemList = await _context.FinanceItems.Where(p => p.Commited && p.UserLogin == userLogin &&
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
            OrderBy = sortParams.OrderBy,
            SessionId = sortParams.SessionId,
            PageNumber = sortParams.PageNumber,
            PageSize = sortParams.PageSize
        };
        await _publishEndpoint.Publish(sortRequest);
    }
}
