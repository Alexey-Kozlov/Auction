using Common.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ProcessiungService.Controllers;


[Authorize]
[ApiController]
[Route("api/finance")]
public class FinanceController : ControllerBase
{


    public FinanceController()
    {

    }



    [HttpPost("AddCredit")]
    public async Task<ApiResponse<decimal>> AddCredit()
    {
        // var userLogin = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login").Select(p => p.Value).FirstOrDefault();
        // using var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
        // var balanceItem = await _context.BalanceItems.Where(p => p.UserLogin == userLogin).OrderByDescending(p => p.ActionDate).FirstOrDefaultAsync();
        // var credit = new BalanceItem
        // {
        //     ActionDate = DateTime.UtcNow,
        //     AuctionId = null,
        //     Balance = (balanceItem?.Balance ?? 0) + creditDTO.amount,
        //     Credit = creditDTO.amount,
        //     Debit = 0,
        //     Reserved = false,
        //     UserLogin = userLogin
        // };

        // await _context.BalanceItems.AddAsync(credit);
        // await _context.SaveChangesAsync();
        // await transaction.CommitAsync();

        // return new ApiResponse<decimal>()
        // {
        //     StatusCode = System.Net.HttpStatusCode.OK,
        //     IsSuccess = true,
        //     Result = credit.Balance
        // };
        return null;

    }

}
