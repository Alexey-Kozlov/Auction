using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using IdentityService.Data;
using IdentityService.Models;
using Common.Utils;

namespace IdentityService.Controllers;

[ApiController]
[Route("finance")]
public class FinanceController : ControllerBase
{

    private readonly ApplicationDbContext _db;
    public FinanceController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpPost("SetCredit")]
    public async Task<ApiResponse<object>> SetCredit([FromBody] RegisterRequestDTO registerRequestDTO)
    {
        var user = await _db.ApplicationUsers.FirstOrDefaultAsync(p => p.UserName.ToLower() == registerRequestDTO.Login.ToLower());
        if (user != null)
        {
            return new ApiResponse<object>()
            {
                StatusCode = HttpStatusCode.BadRequest,
                IsSuccess = false,
                ErrorMessages = ["Такой пользователь уже есть в БД"],
                Result = null
            };
        }
        return null;
    }

    [HttpPost("SetDebit")]
    public async Task<ApiResponse<object>> SetDebit([FromBody] LoginRequestDTO loginRequestDTO)
    {
        var user = await _db.ApplicationUsers.FirstOrDefaultAsync(p => p.Email.ToLower() == loginRequestDTO.Login.ToLower());
        if (user == null)
        {
            return new ApiResponse<object>()
            {
                StatusCode = HttpStatusCode.BadRequest,
                IsSuccess = false,
                ErrorMessages = ["Ошибка пользователя или пароля"],
                Result = new LoginResponseDTO()
            };
        }
        return null;
    }

}
