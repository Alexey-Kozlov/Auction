using System.Security.Claims;
using Common.Utils;
using Common.Contracts;
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
        var balanceItem = await _context.FinanceItems.Where(p => p.UserLogin == userLogin &&
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
        var balanceItemList = _context.FinanceItems.Where(p => p.UserLogin == userLogin &&
            p.Status != FinanceRecordStatus.Баланс)
            .OrderByDescending(p => p.ActionDate) as IQueryable<FinanceItem>;
        var pageCount = 0;
        var itemsCount = await balanceItemList.CountAsync();
        var result = await balanceItemList.Skip((pagedParams.PageNumber - 1) * pagedParams.PageSize)
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
                ItemId = item.Id,
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

}
