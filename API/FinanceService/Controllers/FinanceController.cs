using System.Security.Claims;
using Common.Utils;
using FinanceService.Data;
using FinanceService.DTO;
using FinanceService.Entities;
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

    public FinanceController(FinanceDbContext context)
    {
        _context = context;
    }

    [HttpGet("GetBalance")]
    public async Task<ApiResponse<int>> GetBalance()
    {
        var userLogin = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login").Select(p => p.Value).FirstOrDefault();
        var balanceItem = await _context.BalanceItems.Where(p => p.UserLogin == userLogin).OrderByDescending(p => p.ActionDate).FirstOrDefaultAsync();

        return new ApiResponse<int>()
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = balanceItem?.Balance ?? 0
        };
    }

    [HttpGet("GetHistory")]
    public async Task<ApiResponse<PagedResult<List<BalanceItem>>>> GetHistory([FromQuery] PagedParamsDTO pagedParams)
    {
        var userLogin = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login").Select(p => p.Value).FirstOrDefault();
        var balanceItemList = _context.BalanceItems.Where(p => p.UserLogin == userLogin && p.Status != RecordStatus.Откат)
            .OrderByDescending(p => p.ActionDate) as IQueryable<BalanceItem>;
        var pageCount = 0;
        var itemsCount = await balanceItemList.CountAsync();
        var result = await balanceItemList.Skip((pagedParams.PageNumber - 1) * pagedParams.PageSize)
            .Take(pagedParams.PageSize)
            .ToListAsync();
        if (itemsCount > 0)
        {
            pageCount = (itemsCount + pagedParams.PageSize - 1) / pagedParams.PageSize;
        }
        return new ApiResponse<PagedResult<List<BalanceItem>>>
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = new PagedResult<List<BalanceItem>>()
            {
                Results = result,
                PageCount = pageCount,
                TotalCount = itemsCount
            }
        };

    }

    [HttpPost("AddCredit")]
    public async Task<ApiResponse<decimal>> AddCredit([FromBody] AddCreditDTO creditDTO)
    {
        var userLogin = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login").Select(p => p.Value).FirstOrDefault();
        using var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
        var balanceItem = await _context.BalanceItems.Where(p => p.UserLogin == userLogin).OrderByDescending(p => p.ActionDate).FirstOrDefaultAsync();
        var credit = new BalanceItem
        {
            ActionDate = DateTime.UtcNow,
            AuctionId = null,
            Balance = (balanceItem?.Balance ?? 0) + creditDTO.amount,
            Credit = creditDTO.amount,
            Debit = 0,
            Status = RecordStatus.Подтверждено,
            UserLogin = userLogin
        };

        await _context.BalanceItems.AddAsync(credit);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return new ApiResponse<decimal>()
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = credit.Balance
        };

    }

}
