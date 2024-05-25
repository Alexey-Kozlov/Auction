using System.Security.Claims;
using Common.Utils;
using Contracts;
using FinanceService.Data;
using FinanceService.DTO;
using FinanceService.Entities;
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
    public async Task<ApiResponse<PagedResult<List<BalanceItemDTO>>>> GetHistory([FromQuery] PagedParamsDTO pagedParams)
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
        var resultDTO = new List<BalanceItemDTO>();
        foreach (var item in result)
        {
            resultDTO.Add(new BalanceItemDTO
            {
                Id = item.AuctionId.HasValue ? item.AuctionId.Value : null,
                ItemId = item.Id,
                UserLogin = item.UserLogin,
                Status = item.Status,
                Credit = item.Credit,
                Debit = item.Debit,
                ActionDate = item.ActionDate,
                Balance = item.Balance
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

        await _publishEndpoint.Publish(new FinanceCreditAdd(creditDTO.amount, userLogin));

        return new ApiResponse<decimal>()
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = credit.Balance
        };

    }

}
